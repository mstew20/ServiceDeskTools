using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels
{
    public class NotificationViewModel : Screen
    {

        System.Timers.Timer timer = new System.Timers.Timer();
        private bool _isVisible = false;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
                if (value)
                {
                    timer.Start();
                }
            }
        }

        public NotificationViewModel()
        {
            timer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
            timer.Elapsed += (s, e) =>
            {
                IsVisible = false;
                timer.Stop();
            };
        }

        public ICommand CloseCommand => new RelayCommand(() => { IsVisible = false; timer.Stop(); });

    }
}
