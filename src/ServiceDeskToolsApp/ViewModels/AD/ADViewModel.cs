using AutoMapper;
using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using Microsoft.Extensions.Logging;
using ServiceDeskToolsApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels
{
	public class ADViewModel : Screen
	{
		private string firstName;
		private string lastName;
		private string empId;
		private string _userName;
		private BindingList<ADUserDisplayModel> searchedUsers = [];
		private ADUserDisplayModel selectedUser;
		private readonly ActiveDirectory _activeDirectory;
		private readonly IWindowManager _windowManager;
		private readonly IMapper _mapper;
		private BindingList<UnlockUserModel> serverLockStatus = [];
		private bool isBusy;
		private bool _isRunniningUnlock;
		private bool _isSearching;

		public string FirstName
		{
			get { return firstName; }
			set
			{
				firstName = value;
				NotifyOfPropertyChange(() => FirstName);
			}
		}
		public string LastName
		{
			get { return lastName; }
			set
			{
				lastName = value;
				NotifyOfPropertyChange(() => LastName);
			}
		}
		public string EmpId
		{
			get { return empId; }
			set
			{
				empId = value;
				NotifyOfPropertyChange(() => EmpId);
			}
		}
		public string UserName
		{
			get { return _userName; }
			set
			{
				_userName = value;
				NotifyOfPropertyChange(() => UserName);
			}
		}
		public BindingList<ADUserDisplayModel> SearchedUsers
		{
			get { return searchedUsers; }
			set
			{
				searchedUsers = value;
				NotifyOfPropertyChange(() => SearchedUsers);
			}
		}
		public ADUserDisplayModel SelectedUser
		{
			get { return selectedUser; }
			set
			{
				selectedUser = value;
				NotifyOfPropertyChange(() => SelectedUser);
				NotifyOfPropertyChange(() => CanUnlockAccount);
				NotifyOfPropertyChange(() => CanRunADUnlockToolAsync);
				NotifyOfPropertyChange(() => CanResetPassword);
				ServerLockStatus = [];
			}
		}
		public bool CanUnlockAccount
		{
			get
			{

				if (SelectedUser != null && SelectedUser.IsLockedOut)
				{
					return true;
				}

				return false;
			}
		}
		public BindingList<UnlockUserModel> ServerLockStatus
		{
			get { return serverLockStatus; }
			set
			{
				serverLockStatus = value;
				NotifyOfPropertyChange(() => ServerLockStatus);
			}
		}
		public bool CanRunADUnlockToolAsync
		{
			get
			{
				if (SelectedUser != null && !isBusy)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public bool CanSearchAsync
		{
			get
			{
				if (isBusy)
				{
					return false;
				}

				return true;
			}
		}
		public bool CanResetPassword
		{
			get
			{
				if (SelectedUser != null)
				{
					return true;
				}

				return false;
			}
		}
		public bool IsRunningUnlock
		{
			get { return _isRunniningUnlock; }
			set
			{
				_isRunniningUnlock = value;
				NotifyOfPropertyChange();

				if (value)
				{
					NotifyOfPropertyChange(() => CanRunADUnlockToolAsync);
					NotifyOfPropertyChange(() => CanSearchAsync);
				}
				else
				{
					NotifyOfPropertyChange(() => CanRunADUnlockToolAsync);
					NotifyOfPropertyChange(() => CanSearchAsync);
					NotifyOfPropertyChange(() => SelectedUser);
					NotifyOfPropertyChange(() => CanUnlockAccount);
				}
			}
		}
		public bool IsSearching
		{
			get { return _isSearching; }
			set
			{
				_isSearching = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(() => CanSearchAsync);
				NotifyOfPropertyChange(() => CanRunADUnlockToolAsync);
			}
		}

		public ICommand SearchCommand => new RelayCommand(async () => await SearchAsync());
		public ICommand OpenMoreInfoCommand => new RelayCommand(async () => await OpenMoreInfo());
		public ICommand OpenRenameCommand => new RelayCommand(async () => await OpenRename());
		public ICommand MustResetCommand { get; }
		public ICommand HardSetCommand { get; }

		public ADViewModel(ActiveDirectory activeDirectory, IWindowManager windowManager, IMapper mapper, ILogger<ADViewModel> logger)
		{
			_activeDirectory = activeDirectory;
			_windowManager = windowManager;
			_mapper = mapper;

			MustResetCommand = new RelayCommand(() => _activeDirectory.Users.SaveProperty(SelectedUser.Path, Property.PasswordLastSet, 0));
			HardSetCommand = new RelayCommand(() => _activeDirectory.Users.SaveProperty(SelectedUser.Path, Property.PasswordLastSet, -1));
		}

		public async Task SearchAsync()
		{
			isBusy = true;
			IsSearching = true;

			if (!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName) || !string.IsNullOrWhiteSpace(EmpId) || !string.IsNullOrWhiteSpace(UserName))
			{
				try
				{
					var users = await Task.Run(() => _activeDirectory.Users.GetUsers(FirstName?.Trim(), LastName?.Trim(), EmpId?.Trim(), UserName?.Trim()));
					var mappedUsers = new List<ADUserDisplayModel>();
					foreach (var item in users)
					{
						mappedUsers.Add(_mapper.Map<ADUserDisplayModel>(item));
					}

					SearchedUsers = new BindingList<ADUserDisplayModel>(mappedUsers);
				}
				catch (Exception ex)
				{
					string errorMessage;
					var vm = new DialogViewModel() { DisplayName = "Error - Domain unavailable" };
					if (ex.Message.Equals("The user name or password is incorrect."))
					{
						errorMessage = ex.Message;
					}
					else
					{
						errorMessage = "Domain is currently unavailable:\n\nPlease verify you are connected to the network and try again.";
					}
					vm.UpdateMessage(errorMessage);
					await _windowManager.ShowDialogAsync(vm, null, null);
				}

				if (SearchedUsers.Count > 0)
				{
					SelectedUser = SearchedUsers[0];
				}
			}
			else
			{
				SearchedUsers = [];
			}

			isBusy = false;
			IsSearching = false;
		}
		public void UnlockAccount()
		{
			if (_activeDirectory.Users.UnlockUser(SelectedUser.Path))
			{
				SelectedUser.IsLockedOut = false;
				NotifyOfPropertyChange(() => CanUnlockAccount);
			}
		}
		public async Task RunADUnlockToolAsync()
		{
			isBusy = true;
			IsRunningUnlock = true;

			ServerLockStatus = [];
			try
			{
				await foreach (var item in _activeDirectory.Users.UnlockOnAllDomainsParallelAsync(selectedUser))
				{
					ServerLockStatus.Add(item);
				}

				foreach (var item in ServerLockStatus)
				{
					if (item.IsUnlocked)
					{
						SelectedUser.IsLockedOut = false;
						break;
					}
				}
			}
			finally
			{
				isBusy = false;
				IsRunningUnlock = false;
			}
		}
		public void ClearSearch()
		{
			FirstName = string.Empty;
			LastName = string.Empty;
			UserName = string.Empty;
			EmpId = string.Empty;
		}
		public async Task ResetPassword()
		{
			var vm = IoC.Get<ResetPasswordViewModel>();
			vm.UpdateInformation(SelectedUser);

			dynamic settings = new ExpandoObject();
			settings.Title = $"{SelectedUser.DisplayName} Password Reset";

			await _windowManager.ShowDialogAsync(vm, null, settings);
		}
		public async Task GetAllLockedUsers()
		{
			var users = _activeDirectory.Users.GetAllLockedUsers();
			var vm = IoC.Get<AllLockedUsersViewModel>();
			vm.UpdateUsers(users);

			dynamic settings = new ExpandoObject();
			settings.ResizeMode = ResizeMode.NoResize;
			settings.Title = "Locked Users";

			await _windowManager.ShowWindowAsync(vm, null, settings);
		}
		public async Task OpenMoreInfo()
		{
			var vm = IoC.Get<UserInfoViewModel>();
			vm.UpdateInfo(selectedUser);

			dynamic settings = new ExpandoObject();
			settings.Title = $"{SelectedUser.DisplayName}'s Information";
			settings.Owner = null;
			settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			await _windowManager.ShowWindowAsync(vm, null, settings);
		}
		private async Task OpenRename()
		{
			var vm = IoC.Get<RenameViewModel>();
			vm.UpdateUserInfo(SelectedUser, _activeDirectory);

			dynamic settings = new ExpandoObject();
			settings.Title = $"Rename {SelectedUser.DisplayName}";
			settings.Owner = null;
			settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			await _windowManager.ShowWindowAsync(vm, null, settings);
		}
	}
}
