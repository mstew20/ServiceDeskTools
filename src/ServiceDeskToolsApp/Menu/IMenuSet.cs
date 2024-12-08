using System.ComponentModel;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.Menu;
public interface IMenuSet
{
	public BindingList<IMenuItem> Items { get; set; }

	public Task Build();
}
