using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
public class ComputerSearchViewModel : Screen
{
	private readonly ActiveDirectory _ad;
	private readonly string _userName;
	private readonly string _pass;
	private readonly string _domain;
	private string _computerName;
	private bool _isSearching;

	public string ComputerName
	{
		get { return _computerName; }
		set
		{
			_computerName = value;
			NotifyOfPropertyChange();
		}
	}
	public BindingList<Computer> Computers { get; set; } = [];
	public bool IsSearching
	{
		get { return _isSearching; }
		set
		{
			_isSearching = value;
			NotifyOfPropertyChange();
			NotifyOfPropertyChange(nameof(CanSearch));
		}
	}
	public bool CanSearch
	{
		get
		{
			if (IsSearching)
			{
				return false;
			}

			return true;
		}
	}

	public ICommand SearchCommand { get; }
	public ICommand CopyCommand { get; }
	public ICommand RemoteCommand { get; }

	public ComputerSearchViewModel(ActiveDirectory ad, ShellViewModel shell)
	{
		_ad = ad;
		_userName = shell.SelectedDomain.Credentials.Username;
		_pass = shell.SelectedDomain.Credentials.Password;
		_domain = shell.SelectedDomain.Credentials.Domain;

		SearchCommand = new RelayCommand(async () => await Search());
		CopyCommand = new DelegateCommand<string>(Clipboard.SetText);
		RemoteCommand = new DelegateCommand<string>(RemoteControl);
	}

	public async Task Search()
	{
		IsSearching = true;
		Computers.Clear();
		var computers = await Task.Run(() => _ad.Computers.FindAll(ComputerName));

		foreach (var computer in computers)
		{
			Computers.Add(computer);
		}
		IsSearching = false;
	}
	public void RemoteControl(string compName)
	{
		ProcessStartInfo psi = new()
		{
			UseShellExecute = false,
			Arguments = compName,
			CreateNoWindow = true,
			FileName = @"C:\Program Files (x86)\Microsoft Configuration Manager\AdminConsole\bin\i386\CmRcViewer.exe",
			LoadUserProfile = true,
			UserName = _userName,
			PasswordInClearText = _pass,
			Domain = _domain
		};

		Process.Start(psi);
	}
}
