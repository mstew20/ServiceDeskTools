using ServiceDeskToolsApp.Commands;
using ServiceDeskToolsApp.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
internal class MenuItemViewModel : IMenuItem
{
    public string Header { get; set; }

    public string GetShortcutText { get; }

    public IList<IMenuItem> Children { get; set; }
    public MenuItemCommand Command { get; set; }

    public MenuItemViewModel(string header, Action command = null, KeyGesture gesture = null)
    {
        command ??= () => { };
        Header = header;
        GetShortcutText = gesture?.GetDisplayStringForCulture(CultureInfo.CurrentCulture) ?? string.Empty;
        Command = new(command, gesture!);
        Children = new BindingList<IMenuItem>();
    }
}
