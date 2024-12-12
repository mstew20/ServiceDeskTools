using AutoMapper;
using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ServiceDeskToolsApp.Menu;
using ServiceDeskToolsApp.Models;
using ServiceDeskToolsApp.Settings;
using ServiceDeskToolsApp.Theming;
using ServiceDeskToolsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceDeskToolsApp;
public class Bootstrapper : BootstrapperBase
{
	public IHost Host { get; private set; }

	public Bootstrapper()
	{
		Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration =>
			{
				var dir = Directory.GetParent(Application.ResourceAssembly.Location).FullName;

				configuration.SetBasePath(dir)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
					.AddEnvironmentVariables();

				Log.Logger = new LoggerConfiguration()
				   .ReadFrom.Configuration(configuration.Build())
				   .Enrich.FromLogContext()
				   .WriteTo.Async(a =>
				   {
					   a.Debug();
					   a.File($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Service Desk Tool\\log-.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}");
				   })
				   .CreateLogger();
			})
			.ConfigureServices((context, services) =>
			{
				ConfigureServices(services);
			})
			.UseSerilog()
			.Build();

		Initialize();
	}

	protected void ConfigureServices(IServiceCollection services)
	{
		services
			.AddSingleton<IWindowManager, MetroWindowManager>()
			.AddSingleton<IEventAggregator, EventAggregator>()
			.AddSingleton<IThemeManager, ThemeManager>()
			.AddSingleton<ActiveDirectory>()
			.AddSingleton<AvailableDomainSet>()
			.AddSingleton<ApplicationSettings>();
		//.AddTransient<ExcelProject>();

		services.AddSingleton<IMapper>(ConfigureAutoMapper());
		services.AddSingleton<IMenuSet, MenuSet>();

		GetType().Assembly.GetTypes()
			.Where(type => type.IsClass)
			.Where(type => type.Name.EndsWith("ViewModel"))
			.ToList()
			.ForEach(viewModelType => services.AddTransient(viewModelType, viewModelType));
	}
	protected override object GetInstance(Type serviceType, string key)
	{
		return Host.Services.GetService(serviceType);
	}
	protected override IEnumerable<object> GetAllInstances(Type serviceType)
	{
		return Host.Services.GetServices(serviceType);
	}
	protected override async void OnStartup(object sender, StartupEventArgs e)
	{
		var config = Host.Services.GetRequiredService<IConfiguration>();
		var logger = Host.Services.GetRequiredService<ILogger<Bootstrapper>>();
		var menu = Host.Services.GetRequiredService<IMenuSet>();
		await menu.Build();

		var appSettings = Host.Services.GetService<ApplicationSettings>();
		var settings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
		appSettings.Initialize(settings.LocalDataFiles, settings.ThemeSettings, settings.SettingsFile);
		SetupLocalAppdata(appSettings);

		var domains = config.GetSection("AvailableDomains").Get<AvailableDomainSet>();
		if (domains.Domains is null)
		{
			domains.Domains = [];
		}
		await SetupDomainSettings(domains.Domains, appSettings);

		Host.Services.GetRequiredService<AvailableDomainSet>().Initialize(domains.Domains);
		SetupActiveDirectory(config["ActiveDirectory:DomainName"], domains.Domains);

		Host.Services.GetRequiredService<IThemeManager>().LoadTheme();
		await DisplayRootViewForAsync<ShellViewModel>();
	}

	private static void SetupLocalAppdata(ApplicationSettings appSettings)
	{
		if (!Directory.Exists(appSettings.LocalDataFiles))
		{
			Directory.CreateDirectory(appSettings.LocalDataFiles);
		}
	}

	protected override void OnExit(object sender, EventArgs e)
	{
		Log.CloseAndFlush();
		base.OnExit(sender, e);
	}

	private void SetupActiveDirectory(string currentDomain, List<AvailableDomain> domains)
	{
		if (domains is null or []) return;
		foreach (var domain in domains)
		{
			if (string.IsNullOrWhiteSpace(domain.DefaultDomain))
			{
				if (!string.IsNullOrWhiteSpace(domain.LdapPath))
				{
					var index = domain.LdapPath.IndexOf('/', 7);
					if (index > 0)
					{
						var ldap = domain.LdapPath[7..index];
						domain.DefaultDomain = ldap;
					}
				}
			}

			Task.Run(() =>
			{
				var _ad = new ActiveDirectory();
				_ad.Initialize(domain.Domain, domain.LdapPath);
				domain.DomainControllers = [.. _ad.GetDomainControllers()];
			});
		}
		var activeDomain = domains.Find(x => x.Domain == currentDomain);
		var ad = Host.Services.GetService<ActiveDirectory>();
		ad.Initialize(activeDomain?.Domain, activeDomain?.LdapPath);
		ad.ChangeCredentials(domains.FirstOrDefault(x => x.Domain == ad.Domain)?.Credentials);
	}
	private static async Task SetupDomainSettings(List<AvailableDomain> domains, ApplicationSettings applicationSettings)
	{
		var file = applicationSettings.SettingsFile;
		if (File.Exists(file))
		{
			using var json = File.OpenRead(file);
			var domainsWithCreds = await JsonSerializer.DeserializeAsync<AvailableDomainSet>(json);
			foreach (var domain in domains)
			{
				var loadedDomain = domainsWithCreds.Domains.FirstOrDefault(x => x.Name == domain.Name);
				if (loadedDomain is null)
				{
					continue;
				}
				if (!string.IsNullOrWhiteSpace(loadedDomain.Credentials.Password))
				{
					var bytes = Convert.FromBase64String(loadedDomain.Credentials.Password);
					var entropy = Encoding.UTF8.GetBytes("salty");
					loadedDomain.Credentials.Password = Encoding.UTF8.GetString(ProtectedData.Unprotect(bytes, entropy, DataProtectionScope.CurrentUser));
				}
				domain.Credentials = loadedDomain.Credentials;
				if (!string.IsNullOrWhiteSpace(loadedDomain.DefaultDomain))
				{
					domain.DefaultDomain = loadedDomain.DefaultDomain;
				}

				if (!string.IsNullOrWhiteSpace(loadedDomain.LdapPath))
				{
					domain.LdapPath = loadedDomain.LdapPath;
				}
			}
		}
	}
	private static IMapper ConfigureAutoMapper()
	{
		var config = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<ActiveDirectoryUser, ADUserDisplayModel>();
		});

		return config.CreateMapper();
	}

}
