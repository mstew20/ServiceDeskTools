using EzActiveDirectory;
using EzActiveDirectory.PasswordGenerator;
using Caliburn.Micro;
using ServiceDeskToolsApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels
{
    public class NewHireViewModel : Screen
    {
        private readonly ActiveDirectory _activeDirectory;
        private readonly IWindowManager _window;
        private BindingList<NewHireUserDisplayMode> _newHires = new();

        public BindingList<NewHireUserDisplayMode> NewHires
        {
            get { return _newHires; }
            set
            {
                _newHires = value;
                NotifyOfPropertyChange(() => NewHires);
            }
        }

        public NewHireViewModel(ActiveDirectory activeDirectory, IWindowManager window)
        {
            _activeDirectory = activeDirectory;
            _window = window;
        }

        public async Task RunNewHireListAsync()
        {
            await NewHireListAsync(true);
        }

        public async Task MassPasswordResetAsync()
        {
            await NewHireListAsync(false);
        }

        private async Task NewHireListAsync(bool isTemp)
        {
            if (!isTemp)
            {
                foreach (var user in NewHires)
                {
                    if (string.IsNullOrWhiteSpace(user.TempPassword))
                    {
                        var vm = IoC.Get<DialogViewModel>();
                        vm.UpdateMessage("Not at all users have a set Password");

                        dynamic settings = new ExpandoObject();
                        settings.Title = "Missing Password";

                        await _window.ShowDialogAsync(vm, null, settings);
                        return;
                    }
                }
            }

            foreach (var user in NewHires)
            {
                //var u = await Task.Run(() => _activeDirectory.NewHireInfo(user.EmployeeID));
                var u = (await Task.Run(() => _activeDirectory.Users.GetUsers("", "", user.EmployeeID, ""))).First();
                user.UserName = u.UserName;
                user.Email = u.Email;
                user.Path = u.Path;

                if (isTemp)
                {
                    user.TempPassword = PasswordGenerator.GeneratePassword(12);
                }

                if (!string.IsNullOrWhiteSpace(user.TempPassword))
                {
                    _activeDirectory.Users.ResetPassword(user.Path, user.TempPassword, isTemp);
                }
            }
        }
    }
}
