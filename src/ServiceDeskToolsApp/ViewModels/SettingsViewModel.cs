using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels;
public class SettingsViewModel : Screen
{
	private readonly IEventAggregator _events;
	private readonly IConfiguration _config;
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

	public SettingsViewModel(AvailableDomainSet availableDomains, IEventAggregator eventAggregator, IConfiguration config)
	{
		Domains = new(availableDomains.Domains);
		_events = eventAggregator;
		_config = config;
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

		var json = File.ReadAllText("appsettings.json");
		JsonNode doc = JsonNode.Parse(json);
		var jsonArray = doc["AvailableDomains"]["Domains"].AsArray();
		jsonArray.Add(new { newDomain.Name, newDomain.Domain, newDomain.LdapPath });
		File.WriteAllText("appsettings.json", doc.ToString());
	}
}
