using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceDeskToolsApp.Views
{
    /// <summary>
    /// Interaction logic for ADView.xaml
    /// </summary>
    public partial class ADView : UserControl
    {
        public ADView()
        {
            InitializeComponent();
            Loaded += ADView_Loaded;
            ClearSearch.Click += ClearSeachClicked;
        }

        private void ClearSeachClicked(object sender, RoutedEventArgs e)
        {
            FirstName.Focus();
        }

        private void ADView_Loaded(object sender, RoutedEventArgs e)
        {
            FirstName.Focus();
        }
    }
}
