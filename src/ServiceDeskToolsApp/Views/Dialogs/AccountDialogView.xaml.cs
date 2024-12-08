using System.Windows.Controls;

namespace ServiceDeskToolsApp.Views
{
	/// <summary>
	/// Interaction logic for SCCMLoginView.xaml
	/// </summary>
	public partial class AccountDialogView : UserControl
	{
		public AccountDialogView()
		{
			InitializeComponent();
			Password.Focus();
		}
	}
}
