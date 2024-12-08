using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceDeskToolsApp.Views;
/// <summary>
/// Interaction logic for PingView.xaml
/// </summary>
public partial class PingView : UserControl
{
    public PingView()
    {
        InitializeComponent();
        Loaded += PingView_Loaded;
    }

    private void PingView_Loaded(object sender, RoutedEventArgs e)
    {
        AddressOrComputerName.Focus();
    }
}
