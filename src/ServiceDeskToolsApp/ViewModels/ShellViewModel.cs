using Caliburn.Micro;
using EzActiveDirectory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDeskToolsApp.Menu;
using ServiceDeskToolsApp.Models;
using ServiceDeskToolsApp.Settings;
using ServiceDeskToolsApp.Theming;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
public class ShellViewModel : Conductor<object>
{
	private readonly ILogger<ShellViewModel> _logger;
	private readonly ActiveDirectory _ad;
	private readonly ApplicationSettings _appSettings;
	private readonly IConfiguration _config;
	private bool _choosingTheme;
	private bool _updateAvailable = false;
	private bool _settingsOpened;

	public AvailableDomain SelectedDomain
	{
		get
		{
			return DomainList.Domains?.First(x => x.Domain == _ad.Domain);
		}
		set
		{
			if (!string.IsNullOrWhiteSpace(value.LdapPath))
			{
				_ad.ChangeLdap(value.LdapPath);
			}
			else
			{
				_ad.ChangeDomain(value.Domain);
			}
			_ad.ChangeCredentials(value.Credentials);
			SelectedController = value.DefaultDomain;
			NotifyOfPropertyChange();
		}
	}
	public string SelectedController
	{
		get => SelectedDomain?.DefaultDomain;
		set
		{
			if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(SelectedDomain.DefaultDomain) && value != SelectedDomain.DefaultDomain)
			{
				string ldap = _ad.LdapPath;
				var index = ldap.IndexOf('/', 7);
				if (index > 0)
				{
					ldap = ldap.Remove(7, index - 7);
					ldap = ldap.Insert(7, value);
				}
				else
				{
					ldap = ldap.Insert(7, $"{value}/");
				}
				SelectedDomain.LdapPath = ldap;
				_ad.ChangeLdap(ldap);
			}
			SelectedDomain.DefaultDomain = value;
			NotifyOfPropertyChange();
		}
	}
	public bool IsUpdateAvailable
	{
		get
		{
			return Notifications.IsVisible;
		}
		set
		{
			Notifications.IsVisible = value;
			if (value)
			{
				UpdateAvailable = true;
			}
		}
	}
	public NotificationViewModel Notifications { get; } = new NotificationViewModel();
	public bool UpdateAvailable
	{
		get { return _updateAvailable; }
		set
		{
			_updateAvailable = value;
			NotifyOfPropertyChange(() => UpdateAvailable);
		}
	}
	public bool ChoosingTheme
	{
		get { return _choosingTheme; }
		set
		{
			_choosingTheme = value;
			NotifyOfPropertyChange(() => ChoosingTheme);
		}
	}
	public bool DomainSettingsOpened
	{
		get => _settingsOpened;
		set
		{
			_settingsOpened = value;
			NotifyOfPropertyChange();
			NotifyOfPropertyChange(() => SelectedDomain);
		}
	}
	public IThemeManager ThemeManager { get; }
	public AvailableDomainSet DomainList { get; }
	public IMenuSet Menu { get; }

	public ICommand UpdateApplicationCommand { get; }
	public ICommand OpenThemePanelCommand { get; }
	public ICommand SaveThemeCommand { get; }
	public ICommand ThemeOnCommand { get; }
	public ICommand ThemeOffCommand { get; }
	public ICommand OpenDomainSettingsCommand { get; }
	public ICommand SaveDomainCredentialsCommand { get; }
	public ICommand ChangeDomainCommand { get; }

	public ShellViewModel(ILogger<ShellViewModel> logger,
		IThemeManager themeManager, ActiveDirectory ad, AvailableDomainSet domains, ApplicationSettings appSettings, IMenuSet menu, IConfiguration config)
	{
		ActivateItemAsync(IoC.Get<ADViewModel>(), new CancellationToken());
		_logger = logger;
		ThemeManager = themeManager;
		_ad = ad;
		DomainList = domains;
		_appSettings = appSettings;
		Menu = menu;
		_config = config;

		UpdateApplicationCommand = new RelayCommand(() => throw new NotImplementedException());
		OpenThemePanelCommand = new RelayCommand(() => ChoosingTheme = true);
		SaveThemeCommand = new RelayCommand(ThemeManager.SaveChanges);
		ThemeOnCommand = new RelayCommand(() => ChangeTheme("Dark"));
		ThemeOffCommand = new RelayCommand(() => ChangeTheme("Light"));
		OpenDomainSettingsCommand = new RelayCommand(() => DomainSettingsOpened = true);
		SaveDomainCredentialsCommand = new RelayCommand(async () => await SaveDomainCredentials());
		ChangeDomainCommand = new RelayCommand(ChangeToNextAvailableDomain);
	}

	public async Task SaveDomainCredentials()
	{
		var domains = new AvailableDomainSet
		{
			Domains = []
		};
		foreach (var dom in DomainList.Domains)
		{
			var d = new AvailableDomain
			{
				Name = dom.Name,
				Domain = dom.Domain,
				DefaultDomain = dom.DefaultDomain,
				LdapPath = dom.LdapPath,
			};
			d.Credentials.Username = dom.Credentials.Username;
			d.Credentials.Password = dom.Credentials.Password;
			d.Credentials.Domain = dom.Credentials.Domain;
			domains.Domains.Add(d);
		}

		foreach (var domain in domains.Domains)
		{
			if (!string.IsNullOrWhiteSpace(domain.Credentials.Password))
			{
				var entropy = Encoding.UTF8.GetBytes(_config["entropy"]);
				var passwordBytes = Encoding.UTF8.GetBytes(domain.Credentials.Password);
				var protect = ProtectedData.Protect(passwordBytes, entropy, DataProtectionScope.CurrentUser);
				domain.Credentials.Password = Convert.ToBase64String(protect);
			}
		}
		using Stream stream = new MemoryStream();
		await JsonSerializer.SerializeAsync(stream, domains, options: new() { WriteIndented = true });
		stream.Position = 0;
		using var file = File.OpenWrite(_appSettings.SettingsFile);
		await stream.CopyToAsync(file);
	}

	protected override void OnViewLoaded(object view)
	{
		IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new UpdateMenuEvent());
		base.OnViewLoaded(view);
	}

	private void ChangeToNextAvailableDomain()
	{
		int index = 0;
		for (int i = 0; i < DomainList.Domains.Count; i++)
		{
			if (SelectedDomain == DomainList.Domains[i])
			{
				index = ++i;
				break;
			}
		}

		if (index == DomainList.Domains.Count)
		{
			index = 0;
		}
		SelectedDomain = DomainList.Domains[index];
	}
	private async Task CloseAppAsync()
	{
		await TryCloseAsync();
	}
	private void ChangeTheme(string theme)
	{
		ThemeManager.CurrentScheme = theme;
	}
}
