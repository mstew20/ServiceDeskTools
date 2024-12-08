using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServiceDeskToolsApp.Settings;
public class ApplicationSettings
{
    public string LocalDataFiles { get; set; }
    public string ThemeSettings { get; set; }
    public string SettingsFile { get; set; }

    public void Initialize(string path, string theme, string settingsFile)
    {
        LocalDataFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);
        ThemeSettings = Path.Combine(LocalDataFiles, theme);
        SettingsFile = Path.Combine(LocalDataFiles, settingsFile);
    }
}
