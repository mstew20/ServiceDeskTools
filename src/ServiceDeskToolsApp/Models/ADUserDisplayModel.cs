using EzActiveDirectory.Models;
using System;
using System.ComponentModel;

namespace ServiceDeskToolsApp.Models
{
	public class ADUserDisplayModel : ActiveDirectoryUser, INotifyPropertyChanged
	{
		private bool _isLockedOut;
		private bool _isExpired;
		private bool _passwordMustBeChanged;
		private bool isActive;

		public new bool IsExpired
		{
			get { return _isExpired; }
			set
			{
				_isExpired = value;
				CallPropertyChanged(nameof(IsExpired));
				CallPropertyChanged(nameof(ExpiredMessage));
			}
		}
		public new bool IsActive
		{
			get => isActive;
			set
			{
				isActive = value;
				CallPropertyChanged(nameof(IsActive));
			}
		}
		public new bool IsLockedOut
		{
			get { return _isLockedOut; }
			set
			{
				_isLockedOut = value;
				CallPropertyChanged(nameof(IsLockedOut));
			}
		}
		public string PasswordLastSetString
		{
			get
			{
				if (PasswordLastSet.Year <= 1600)
				{
					return "";
				}
				else
				{
					return $"{PasswordLastSet:MM/dd/yyyy h:mm tt} {(PasswordNeverExpires ? "" : $"({PasswordExpiryDate.Subtract(DateTime.Now).Days})")}";
				}
			}
		}
		public string ExpiredMessage
		{
			get
			{
				if (IsExpired)
				{
					return "(Expired)";
				}

				if (AccountExpireDate is not null && AccountExpireDate?.Subtract(DateTime.Now).Days < 5)
				{
					return "(Expires Soon)";
				}

				return string.Empty;
			}
		}
		public bool PasswordMustBeChanged
		{
			get
			{
				if (PasswordLastSet.Year <= 1600)
				{
					_passwordMustBeChanged = true;
				}
				else
				{
					_passwordMustBeChanged = false;
				}

				return _passwordMustBeChanged;
			}
			set
			{
				_passwordMustBeChanged = value;
				if (_passwordMustBeChanged)
				{
					PasswordLastSet = DateTime.MinValue;
				}
				else
				{
					PasswordLastSet = DateTime.Now;
					PasswordExpiryDate = PasswordLastSet.AddDays(90);
				}
				CallPropertyChanged(nameof(PasswordLastSet));
				CallPropertyChanged(nameof(PasswordLastSetString));
				CallPropertyChanged(nameof(PasswordMustBeChanged));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void CallPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
	}
}
