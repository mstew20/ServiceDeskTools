using ControlzEx.Theming;

namespace ServiceDeskToolsApp.Theming
{
    public interface IThemeManager
    {
        Theme[] Colors { get; }
        Theme CurrentColor { get; set; }
        string CurrentScheme { get; set; }
        string CurrentTheme { get; }

        void SaveChanges();
        void LoadTheme();
    }
}