using Caliburn.Micro;
using System;

namespace ServiceDeskToolsApp.ViewModels
{
	public class AccountDialogViewModel : Screen
	{
		private string _userName = Environment.UserName;
		private string _password;

		public string Password
		{
			get { return _password; }
			set
			{
				_password = value;
				NotifyOfPropertyChange();
			}
		}
		public string UserName
		{
			get { return _userName; }
			set
			{
				_userName = value;
				NotifyOfPropertyChange();
			}
		}


		public void Connect()
		{
			TryCloseAsync(true);
		}

		public void AsMe()
		{
			UserName = "";
			TryCloseAsync(true);
		}

		public void Cancel()
		{
			TryCloseAsync(false);
		}
	}
}
