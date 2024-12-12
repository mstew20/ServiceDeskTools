using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Input;

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

	public ICommand DeleteCommand { get; }

	public SettingsViewModel(AvailableDomainSet availableDomains, IEventAggregator eventAggregator, IConfiguration config)
	{
		Domains = new(availableDomains.Domains);
		_events = eventAggregator;
		_config = config;

		DeleteCommand = new DelegateCommand<AvailableDomain>(async (item) => await Delete(item));
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
		var message = new UpdateDomainListEvent(newDomain, UpdateAction.Add);
		await _events.PublishOnUIThreadAsync(message);

		Domain = string.Empty;
		Name = string.Empty;
		Ldap = string.Empty;

		var json = await File.ReadAllTextAsync("appsettings.json");
		JsonNode doc = JsonNode.Parse(json);
		var jsonArray = doc["AvailableDomains"]["Domains"].AsArray();
		jsonArray.Add(new { newDomain.Name, newDomain.Domain, newDomain.LdapPath });
		await File.WriteAllTextAsync("appsettings.json", doc.ToString());
	}

	private async Task Delete(AvailableDomain toDelete)
	{
		Domains.Remove(toDelete);
		var message = new UpdateDomainListEvent(toDelete, UpdateAction.Delete);
		await _events.PublishOnUIThreadAsync(message);

		var json = await File.ReadAllTextAsync("appsettings.json");
		JsonNode doc = JsonNode.Parse(json);
		var jsonArray = doc["AvailableDomains"]["Domains"].AsArray();
		jsonArray.Remove(jsonArray.Where(x => x["Name"].GetValue<string>() == toDelete.Name).First());
		await File.WriteAllTextAsync("appsettings.json", doc.ToString());
	}
}
