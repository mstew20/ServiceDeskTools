using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ServiceDeskToolsApp.Commands;
public class MenuItemCommand : ICommand
{
    private readonly Action _action;

    public KeyGesture Gesture { get; }

    public event EventHandler CanExecuteChanged = (a,o) => { };

    public MenuItemCommand(Action action, KeyGesture gesture)
    {
        _action = action;
        Gesture = gesture;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        _action();
    }
}
