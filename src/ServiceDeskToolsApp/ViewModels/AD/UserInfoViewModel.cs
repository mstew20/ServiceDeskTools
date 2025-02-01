using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Menu;
using ServiceDeskToolsApp.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels
{
	public class UserInfoViewModel : Screen, IHandle<ADAddGroupEvent>
	{
		#region Private Members

		private ADUserDisplayModel _user;
		private readonly ActiveDirectory _activeDirectory;
		private readonly IWindowManager _windowManager;
		private BindingList<ActiveDirectoryGroup> _userGroups;
		private List<ActiveDirectoryGroup> _unfiltedGroups;
		private string _userLicense;
		private string _filterTerm;

		#endregion

		#region Public Properties

		public ADUserDisplayModel User
		{
			get { return _user; }
			private set
			{
				_user = value;
				NotifyOfPropertyChange(() => User);
			}
		}
		public BindingList<ActiveDirectoryGroup> UserGroups
		{
			get { return _userGroups; }
			set
			{
				_userGroups = value;
				NotifyOfPropertyChange(() => UserGroups);
			}
		}
		public string UserLicense
		{
			get { return _userLicense; }
			set
			{
				_userLicense = value;
				NotifyOfPropertyChange(() => UserLicense);
			}
		}
		public string CreatedDate => User.DateCreated.ToLocalTime().ToString();
		public string ModifiedDate => User.DateModified.ToLocalTime().ToString();
		public string FilterTerm
		{
			get { return _filterTerm; }
			set
			{
				_filterTerm = value;
				NotifyOfPropertyChange();
				FilterGroups();
			}
		}

		#endregion

		#region Command

		public ICommand RemoveGroupCommand { get; }
		public ICommand OpenGroupInfoCommand { get; }

		#endregion

		#region Constructor

		public UserInfoViewModel(ActiveDirectory activeDirectory, IWindowManager windowManager, IEventAggregator events)
		{
			_activeDirectory = activeDirectory;
			_windowManager = windowManager;
			_unfiltedGroups = new();

			events.SubscribeOnUIThread(this);
			RemoveGroupCommand = new DelegateCommand<ActiveDirectoryGroup>(RemoveGroup);
			OpenGroupInfoCommand = new DelegateCommand<ActiveDirectoryGroup>(async (g) => await OpenGroupInfo(g));
		}

		#endregion

		#region Public Methods

		public async Task EnableExpiredAccount()
		{
			if (await ShowDialog())
			{
				_activeDirectory.Users.NeverExpires(User.Path);
				User.AccountExpireDate = null;
				User.IsExpired = false;
				NotifyOfPropertyChange(() => User);
			}
		}
		public async Task ExpireAccount()
		{
			if (await ShowDialog())
			{
				_activeDirectory.Users.ExpireNow(User.Path);
				User.IsExpired = true;
				NotifyOfPropertyChange(() => User);
			}
		}
		public void UpdateInfo(ADUserDisplayModel user)
		{
			User = user;
		}
		public void SaveChanges()
		{
			_activeDirectory.Users.SaveProperty(User.Path, "info", User.Notes);
		}
		public async Task AddGroups()
		{
			var vm = IoC.Get<AddGroupsViewModel>();
			vm.User = User.Path;
			dynamic settings = new ExpandoObject();
			settings.Title = "Add Group(s)";
			settings.Width = 600;
			settings.Height = 400;
			settings.SizeToContent = SizeToContent.Manual;
			await _windowManager.ShowWindowAsync(vm, null, settings);
		}
		public void DisableAccount()
		{
			if (_activeDirectory.Users.DisableAccount(User.Path))
			{
				User.IsActive = false;
				User.AccountControl = (int)AccountFlag.Disable;
				NotifyOfPropertyChange(() => User);
			}
		}
		public void EnableAccount()
		{
			if (_activeDirectory.Users.EnableAccount(User.Path))
			{
				User.IsActive = true;
				User.AccountControl = (int)AccountFlag.Normal;
				NotifyOfPropertyChange(() => User);
			}
		}

		#endregion

		private void RemoveGroup(ActiveDirectoryGroup groupName)
		{
			if (_activeDirectory.Groups.RemoveUser(User.Path, groupName.Path))
			{
				UserGroups.Remove(groupName);
				_unfiltedGroups.Remove(groupName);
			}
		}
		private async Task<bool> ShowDialog()
		{
			var vm = IoC.Get<DialogResponseViewModel>();
			vm.Message = "Are you sure you want to do this?";

			dynamic settings = new ExpandoObject();
			settings.Title = "Are you Sure?";

			var result = await _windowManager.ShowDialogAsync(vm, null, settings);
			return result;
		}
		private string GetLicenseType()
		{
			StringBuilder sb = new();

			foreach (var group in UserGroups)
			{
				if (group.Name.Contains("GRP-IdentityNow-Assign"))
				{
					sb.Append($"{group.Name[^2..]}, ");
				}
			}

			return sb.ToString().Trim(new char[] { ',', ' ' });
		}
		private void FilterGroups()
		{
			if (string.IsNullOrWhiteSpace(FilterTerm))
			{
				UserGroups = new(_unfiltedGroups);
			}
			else
			{
				UserGroups = new(_unfiltedGroups.Where(x => x.Name.ToLower().Contains(FilterTerm.ToLower())).ToList());
			}
		}
		private async Task OpenGroupInfo(ActiveDirectoryGroup group)
		{
			var vm = IoC.Get<GroupSearchViewModel>();
			var grp = _activeDirectory.Groups.Get(group.Path);
			vm.SearchedGroups.Add(grp);
			vm.SelectedGroup = grp;

			await ApplicationMenuCommands.OpenGroupSearch(vm);
		}
		private void GetUserGroups()
		{
			_unfiltedGroups = _activeDirectory.Users.GetGroups(User.Path);
			UserGroups = new BindingList<ActiveDirectoryGroup>(_unfiltedGroups);
		}

		protected override void OnViewLoaded(object view)
		{
			GetUserGroups();
			UserLicense = GetLicenseType();

			base.OnViewLoaded(view);
		}

		public Task HandleAsync(ADAddGroupEvent message, CancellationToken cancellationToken)
		{
			GetUserGroups();
			FilterGroups();
			return Task.CompletedTask;
		}
	}
}
