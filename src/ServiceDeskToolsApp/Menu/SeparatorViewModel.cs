using ServiceDeskToolsApp.Commands;
using ServiceDeskToolsApp.Menu;
using System;
using System.Collections.Generic;

namespace ServiceDeskToolsApp.ViewModels;
public class SeparatorViewModel : IMenuItem
{
    public String Header { get; set; }
    public String GetShortcutText { get; }
    public IList<IMenuItem> Children { get; set; } = [];
    public MenuItemCommand Command { get; set; }
}
