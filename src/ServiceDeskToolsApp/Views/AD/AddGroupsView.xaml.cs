using System.Windows.Controls;

namespace ServiceDeskToolsApp.Views;
/// <summary>
/// Interaction logic for AddGroupsView.xaml
/// </summary>
public partial class AddGroupsView : UserControl
{
	public AddGroupsView()
	{
		InitializeComponent();
		GroupSearchTerm.Focus();
	}
}
