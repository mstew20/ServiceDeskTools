using Caliburn.Micro;
using EzActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels
{
	public class LoggedInUserViewModel : Screen
	{
		private string _computerName;
		private string _result;
		private bool _canRun = true;
		private readonly ShellViewModel _shell;

		public string ComputerName
		{
			get { return _computerName; }
			set
			{
				_computerName = value;
				NotifyOfPropertyChange();
			}
		}
		public string Result
		{
			get { return _result; }
			set
			{
				_result = value;
				NotifyOfPropertyChange();
			}
		}
		public bool CanRun
		{
			get { return _canRun; }
			set
			{
				_canRun = value;
				NotifyOfPropertyChange();
			}
		}

		public LoggedInUserViewModel(ShellViewModel shell)
		{
			_shell = shell;
		}

		public async Task Run()
		{
			Result = string.Empty;
			CanRun = false;
			await Task.Run(async () =>
			{

				ProcessStartInfo psi = new()
				{
					//WorkingDirectory = "",
					UseShellExecute = false,
					Arguments = $"/c query session /server:{ComputerName}",
					CreateNoWindow = true,
					FileName = "cmd.exe",
					LoadUserProfile = true,
					UserName = _shell.SelectedDomain.Credentials.Username,
					PasswordInClearText = _shell.SelectedDomain.Credentials.Password,
					Domain = _shell.SelectedDomain.Credentials.Domain,
					RedirectStandardOutput = true
				};

				var p = Process.Start(psi);
				p.WaitForExit();
				var r = await p.StandardOutput.ReadToEndAsync();
				if (string.IsNullOrWhiteSpace(r))
				{
					r = $"Unable to connect to RPC Service for {ComputerName}";
				}

				Result = r;
			});
			CanRun = true;
		}

		protected override void OnViewLoaded(object view)
		{
			((dynamic)view).ComputerName.Focus();
			base.OnViewLoaded(view);
		}
	}
}
