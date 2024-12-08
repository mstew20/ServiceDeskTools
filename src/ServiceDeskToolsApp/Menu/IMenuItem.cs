using ServiceDeskToolsApp.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ServiceDeskToolsApp.Menu;
public interface IMenuItem
{
    public string Header { get; set; }
    public string GetShortcutText { get; }
    public IList<IMenuItem> Children { get; set; }
    public MenuItemCommand Command { get; set; }
}
