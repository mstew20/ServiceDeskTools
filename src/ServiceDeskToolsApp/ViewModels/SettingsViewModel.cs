using Caliburn.Micro;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels;
public class SettingsViewModel : Screen
{
	private readonly IEventAggregator _events;
	private string _name;
	private string _domain;
	private string _ldap;

	public ObservableCollection<AvailableDomain> Domains { get; set; }
	public string Name
	{
		get => _name;
		set
		{
			_name = value;
			NotifyOfPropertyChange();
		}
	}
	public string Domain
	{
		get => _domain;
		set
		{
			_domain = value;
			NotifyOfPropertyChange();
		}
	}
	public string Ldap
	{
		get => _ldap;
		set
		{
			_ldap = value;
			NotifyOfPropertyChange();
		}
	}

	public SettingsViewModel(AvailableDomainSet availableDomains, IEventAggregator eventAggregator)
	{
		Domains = new(availableDomains.Domains);
		_events = eventAggregator;
	}

	public async Task Add()
	{
		AvailableDomain newDomain = new()
		{
			Domain = Domain,
			Name = Name,
			LdapPath = Ldap,
		};

		Domains.Add(newDomain);
		var message = new UpdateDomainListEvent(newDomain);
		await _events.PublishOnUIThreadAsync(message);

		Domain = string.Empty;
		Name = string.Empty;
		Ldap = string.Empty;
	}
}
