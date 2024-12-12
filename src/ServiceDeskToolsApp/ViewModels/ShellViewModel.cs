using Caliburn.Micro;
using EzActiveDirectory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Menu;
using ServiceDeskToolsApp.Models;
using ServiceDeskToolsApp.Settings;
using ServiceDeskToolsApp.Theming;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
public class ShellViewModel : Conductor<object>, IHandle<UpdateDomainListEvent>
{
	private readonly ILogger<ShellViewModel> _logger;
	private readonly ActiveDirectory _ad;
	private readonly ApplicationSettings _appSettings;
	private readonly IConfiguration _config;
	private bool _choosingTheme;
	private bool _updateAvailable = false;
	private bool _domainSettingsOpened;
	private bool _settingsPanelOpened;

	public AvailableDomain SelectedDomain
	{
		get
		{
			return DomainListSet.Domains?.First(x => x.Domain == _ad.Domain);
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
	public NotificationViewModel Notifications { get; } = new();
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
		get => _domainSettingsOpened;
		set
		{
			_domainSettingsOpened = value;
			NotifyOfPropertyChange();
			NotifyOfPropertyChange(() => SelectedDomain);
		}
	}
	public bool SettingsPanelOpened
	{
		get { return _settingsPanelOpened; }
		set
		{
			_settingsPanelOpened = value;
			NotifyOfPropertyChange();
		}
	}
	public SettingsViewModel Settings { get; }
	public ObservableCollection<AvailableDomain> DomainList { get; }
	public IThemeManager ThemeManager { get; }
	public AvailableDomainSet DomainListSet { get; }
	public IMenuSet Menu { get; }

	public ICommand UpdateApplicationCommand { get; }
	public ICommand OpenSettingsCommand { get; }
	public ICommand OpenThemePanelCommand { get; }
	public ICommand SaveThemeCommand { get; }
	public ICommand ThemeOnCommand { get; }
	public ICommand ThemeOffCommand { get; }
	public ICommand OpenDomainSettingsCommand { get; }
	public ICommand SaveDomainCredentialsCommand { get; }
	public ICommand ChangeDomainCommand { get; }

	public ShellViewModel(ILogger<ShellViewModel> logger,
		IThemeManager themeManager,
		ActiveDirectory ad,
		AvailableDomainSet domains,
		ApplicationSettings appSettings,
		IMenuSet menu,
		IConfiguration config,
		IEventAggregator eventAggregator)
	{
		_logger = logger;
		_ad = ad;
		_appSettings = appSettings;
		_config = config;

		ThemeManager = themeManager;
		DomainListSet = domains;
		Menu = menu;
		Settings = IoC.Get<SettingsViewModel>();
		DomainList = new(DomainListSet.Domains);

		UpdateApplicationCommand = new RelayCommand(() => throw new NotImplementedException());
		OpenThemePanelCommand = new RelayCommand(() => ChoosingTheme = true);
		SaveThemeCommand = new RelayCommand(ThemeManager.SaveChanges);
		ThemeOnCommand = new RelayCommand(() => ChangeTheme("Dark"));
		ThemeOffCommand = new RelayCommand(() => ChangeTheme("Light"));
		OpenDomainSettingsCommand = new RelayCommand(() => DomainSettingsOpened = true);
		SaveDomainCredentialsCommand = new RelayCommand(async () => await SaveDomainCredentials());
		ChangeDomainCommand = new RelayCommand(ChangeToNextAvailableDomain);
		OpenSettingsCommand = new RelayCommand(() => SettingsPanelOpened = true);

		eventAggregator.SubscribeOnUIThread(this);

		ActivateItemAsync(IoC.Get<ADViewModel>(), new CancellationToken());
	}

	public async Task SaveDomainCredentials()
	{
		var domains = new AvailableDomainSet
		{
			Domains = []
		};
		foreach (var dom in DomainListSet.Domains)
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
		for (int i = 0; i < DomainListSet.Domains.Count; i++)
		{
			if (SelectedDomain == DomainListSet.Domains[i])
			{
				index = ++i;
				break;
			}
		}

		if (index == DomainListSet.Domains.Count)
		{
			index = 0;
		}
		SelectedDomain = DomainListSet.Domains[index];
	}
	private void ChangeTheme(string theme)
	{
		ThemeManager.CurrentScheme = theme;
	}

	Task IHandle<UpdateDomainListEvent>.HandleAsync(UpdateDomainListEvent message, CancellationToken cancellationToken)
	{
		switch (message.Action)
		{
			case UpdateAction.Add:
				DomainList.Add(message.Domain);
				DomainListSet.Domains.Add(message.Domain);
				Task.Run(() =>
				{
					var ad = new ActiveDirectory();
					ad.Initialize(message.Domain.Domain, message.Domain.LdapPath);
					var controllers = ad.GetDomainControllers().ToArray();
					message.Domain.DomainControllers = controllers;
					message.Domain.DefaultDomain = controllers[0];
				});
				break;
			case UpdateAction.Delete:
				DomainList.Remove(message.Domain);
				DomainListSet.Domains.Remove(message.Domain);
				break;
			default:
				break;
		}

		return Task.CompletedTask;
	}
}
