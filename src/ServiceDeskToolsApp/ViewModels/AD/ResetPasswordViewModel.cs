using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using EzActiveDirectory.PasswordGenerator;
using Microsoft.Extensions.Logging;
using ServiceDeskToolsApp.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels
{
    public class ResetPasswordViewModel : Screen
    {
        #region Private Members

        private readonly ActiveDirectory _activeDirectory;
        private readonly IWindowManager _windowManager;
        private readonly ILogger<ResetPasswordViewModel> _logger;
        private string confirmPassword;
        private string newPassword;
        private bool mustChangePassword = true;
        private bool _isLoading;

        #endregion

        #region Public Properties

        public string NewPassword
        {
            get { return newPassword; }
            set
            {
                newPassword = value;
                NotifyOfPropertyChange(() => NewPassword);
            }
        }
        public string ConfirmPassword
        {
            get { return confirmPassword; }
            set
            {
                confirmPassword = value;
                NotifyOfPropertyChange(() => ConfirmPassword);
            }
        }
        public bool MustChangePassword
        {
            get { return mustChangePassword; }
            set
            {
                mustChangePassword = value;
                NotifyOfPropertyChange(() => MustChangePassword);
            }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange();
            }
        }

        public ADUserDisplayModel User { get; private set; }

        #endregion

        #region Constructor

        public ResetPasswordViewModel(ActiveDirectory activeDirectory, IWindowManager windowManager, ILogger<ResetPasswordViewModel> logger)
        {
            _activeDirectory = activeDirectory;
            _windowManager = windowManager;
            _logger = logger;
        }

        #endregion

        #region Commands

        public ICommand ResetCommand => new RelayCommand(async () => await ResetPassword());

        #endregion

        #region Public Methods

        public async Task Cancel()
        {
            await TryCloseAsync();
        }

        public async Task ResetPassword()
        {
            if (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(confirmPassword) && NewPassword == ConfirmPassword)
            {
                try
                {
                    IsLoading = true;
                    await Task.Run(() => _activeDirectory.Users.ResetPassword(User.Path, NewPassword, MustChangePassword));
                    User.PasswordMustBeChanged = mustChangePassword;

                    await TryCloseAsync();
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    if (ex.InnerException != null)
                    {
                        error = ex.InnerException.Message;
                        error = error.Remove(error.IndexOf('('));
                    }
                    string message = $"Windows cannot complete the password change for {User.DisplayName} because:\n\n{error}";//The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements.";

                    if (error.Contains("access is denied", StringComparison.OrdinalIgnoreCase))
                    {
                        message += "\n\n Please try using your admin account!";
                    }

                    _logger.LogInformation(ex, "Unable to reset password");
                    await ShowDialogWindow("Error", message);
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else
            {
                await ShowDialogWindow("Error", "Password and Confirm Password do not match or are empty!");
            }
        }

        public void GeneratePassword()
        {
            NewPassword =  PasswordGenerator.GeneratePassword(12);
            ConfirmPassword = NewPassword;
        }
        public void UpdateInformation(ADUserDisplayModel user)
        {
            User = user;
        }

        #endregion

        private async Task ShowDialogWindow(string title, string message)
        {
            var window = IoC.Get<DialogViewModel>();
            window.UpdateMessage(message);

            dynamic settings = new ExpandoObject();
            settings.Title = title;
            settings.ResizeMode = ResizeMode.NoResize;

            await _windowManager.ShowDialogAsync(window, null, settings);
        }
    }
}
