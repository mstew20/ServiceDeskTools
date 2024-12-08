﻿using System.Windows;
using System.Windows.Controls;

namespace ServiceDeskToolsApp.Menu;
public class MenuItemContainerTemplateSelector : ItemContainerTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
    {
        var key = new DataTemplateKey(item.GetType());
        return (DataTemplate)parentItemsControl.FindResource(key);
    }
}
