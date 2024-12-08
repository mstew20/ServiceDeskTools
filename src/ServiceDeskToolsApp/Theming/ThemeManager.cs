using Caliburn.Micro;
using ControlzEx.Theming;
using _manager = ControlzEx.Theming.ThemeManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using ServiceDeskToolsApp.Settings;

namespace ServiceDeskToolsApp.Theming
{
    public class ThemeManager : PropertyChangedBase, IThemeManager
    {
        private string _currentScheme = _manager.Current.DetectTheme().BaseColorScheme;
        private Theme _currentColor = _manager.Current.DetectTheme();
        private string file;

        public string CurrentScheme
        {
            get { return _currentScheme; }
            set
            {
                _currentScheme = value;
                NotifyOfPropertyChange(() => CurrentScheme);
                ChangeTheme();
            }
        }
        public string CurrentTheme => $"{CurrentScheme}.{CurrentColor.ColorScheme}";
        public Theme CurrentColor
        {
            get { return _currentColor; }
            set
            {
                _currentColor = value;
                NotifyOfPropertyChange(() => CurrentColor);
                ChangeTheme();
            }
        }
        public Theme[] Colors
        {
            get
            {
                var themes = _manager.Current.Themes.ToList();
                themes.RemoveAll(theme => theme.BaseColorScheme == "Light");
                return themes.OrderBy(color => color.ColorScheme).ToArray();
            }
        }

        public ThemeManager(ApplicationSettings appSettings)
        {
            file = appSettings.ThemeSettings;
        }

        private void ChangeTheme()
        {
            _manager.Current.ChangeTheme(Application.Current, CurrentTheme);
        }
        public void SaveChanges()
        {
            File.WriteAllText(file, CurrentTheme);
        }
        public void LoadTheme()
        {
            if (File.Exists(file))
            {
                string themeSettings = File.ReadAllText(file);
                if (!string.IsNullOrWhiteSpace(themeSettings))
                {
                    var settings = themeSettings.Split('.');

                    CurrentScheme = settings[0];
                    CurrentColor = Colors.FirstOrDefault(color => color.ColorScheme == settings[1]);  
                }
            }
        }
    }
}
