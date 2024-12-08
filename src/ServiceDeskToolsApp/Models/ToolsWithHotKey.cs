using Caliburn.Micro;

namespace ServiceDeskToolsApp.Models;
public class ToolsWithHotKey : Screen
{
    private string _header;
    private string _filName;
    private string _arguments;
    private string _workingDirectory;

    public Hotkey HotKey { get; set; }
    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            NotifyOfPropertyChange();
        }
    }
    public string FileName
    {
        get => _filName;
        set
        {
            _filName = value;
            NotifyOfPropertyChange();
        }
    }
    public string Arguemnts
    {
        get => _arguments;
        set
        {
            _arguments = value;
            NotifyOfPropertyChange();
        }
    }
    public string WorkingDirectory
    {
        get => _workingDirectory;
        set
        {
            _workingDirectory = value;
            NotifyOfPropertyChange();
        }
    }
}
