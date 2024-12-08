using Caliburn.Micro;

namespace ServiceDeskToolsApp.ViewModels.Tools;
public class AddNewToolViewModel : Screen
{
    private string _name;
    private string _fileName;
    private string _arguments;
    private string _workingDirectory;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            NotifyOfPropertyChange();
        }
    }
    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
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

    public void Save()
    {
        TryCloseAsync(true);
    }

    public void Cancel()
    {
        TryCloseAsync(false);
    }
}
