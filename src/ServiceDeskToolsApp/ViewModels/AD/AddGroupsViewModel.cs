using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ServiceDeskToolsApp.Events;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ServiceDeskToolsApp.ViewModels;
public class AddGroupsViewModel : Screen
{
	private readonly ActiveDirectory _ad;
	private readonly IEventAggregator _events;
	private string _groupSearchTerm;
	private ActiveDirectoryGroup _selectedGroup;
	private ActiveDirectoryGroup _selectedGroupToAdd;
	private bool _isSearching;
	private bool _isLoading;

	public string GroupSearchTerm
	{
		get => _groupSearchTerm;
		set
		{
			_groupSearchTerm = value;
			NotifyOfPropertyChange();
		}
	}
	public ActiveDirectoryGroup SelectedGroup
	{
		get => _selectedGroup;
		set
		{
			_selectedGroup = value;
			NotifyOfPropertyChange();
			NotifyOfPropertyChange(() => CanAddGroup);
		}
	}
	public ActiveDirectoryGroup SelectedGroupToAdd
	{
		get => _selectedGroupToAdd;
		set
		{
			_selectedGroupToAdd = value;
			NotifyOfPropertyChange();
			NotifyOfPropertyChange(() => CanRemoveGroup);
		}
	}
	public bool IsSearching
	{
		get { return _isSearching; }
		set
		{
			_isSearching = value;
			NotifyOfPropertyChange();
		}
	}
	public bool IsLoading
	{
		get => _isLoading;
		set
		{
			_isLoading = value;
			NotifyOfPropertyChange();
		}
	}
	public bool CanRemoveGroup => SelectedGroupToAdd != null;
	public bool CanAddGroup => SelectedGroup != null;

	public ObservableCollection<ActiveDirectoryGroup> GroupsSearch { get; set; } = [];
	public ObservableCollection<ActiveDirectoryGroup> GroupsToAdd { get; set; } = [];
	public string User { get; set; }

	public ICommand SearchCommand { get; }

	public AddGroupsViewModel(ActiveDirectory ad, IEventAggregator events)
	{
		_ad = ad;

		SearchCommand = new RelayCommand(async () => await SearchGroup());
		_events = events;
	}

	public async Task SearchGroup()
	{
		IsSearching = true;
		GroupsSearch.Clear();

		var results = await Task.Run(() => _ad.Groups.Find(GroupSearchTerm));

		foreach (var group in results)
		{
			GroupsSearch.Add(group);
		}
		IsSearching = false;
	}
	public void AddGroup()
	{
		GroupsToAdd.Add(SelectedGroup);
		GroupsSearch.Remove(SelectedGroup);
	}
	public void RemoveGroup()
	{
		if (!string.IsNullOrWhiteSpace(GroupSearchTerm) && SelectedGroupToAdd.Name.Contains(GroupSearchTerm, System.StringComparison.InvariantCultureIgnoreCase))
		{
			GroupsSearch.Add(SelectedGroupToAdd);
		}
		GroupsToAdd.Remove(SelectedGroupToAdd);
	}
	public async Task ApplyGroups()
	{
		IsLoading = true;
		var failures = new ConcurrentBag<string>();
		List<Task<(bool success, string groupName)>> tasks = [];
		foreach (var group in GroupsToAdd)
		{
			tasks.Add(Task.Run(() => (_ad.Groups.AddUser(User, group.Path), group.Name)));
		}

		await foreach (var completedTask in Task.WhenEach(tasks))
		{
			var result = await completedTask;
			if (!result.success)
			{
				failures.Add(result.groupName);
			}
		};

		IsLoading = false;
		if (failures.Count > 0)
		{
			await DialogManager.ShowMessageAsync((MetroWindow)GetView(), "Unable to add", $"Unable to add group(s) {string.Join(", ", failures)} to account");
		}
		var message = new ADAddGroupEvent();
		await _events.PublishOnUIThreadAsync(message);
		await TryCloseAsync();
	}
	public async Task CopyFromUser()
	{
		IsLoading = true;
		var username = await DialogManager.ShowInputAsync((MetroWindow)this.GetView(), "Copy User", "Enter username to copy");
		if (!string.IsNullOrWhiteSpace(username))
		{
			GroupsToAdd.Clear();
			var user = await Task.Run(() => _ad.Users.GetUsers("", "", "", username).FirstOrDefault(x => x.UserName == username));
			if (user is not null)
			{
				var groups = await Task.Run(() => _ad.Users.GetGroups(user.Path));
				foreach (var group in groups)
				{
					GroupsToAdd.Add(group);
				}
			}
			else
			{
				await DialogManager.ShowMessageAsync((MetroWindow)this.GetView(), "Not Found", $"User with username {username} was not found");
			}
			IsLoading = false;
		}
	}
}
