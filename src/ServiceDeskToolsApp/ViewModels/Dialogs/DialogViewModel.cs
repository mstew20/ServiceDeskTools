using Caliburn.Micro;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels
{
	public class DialogViewModel : Screen
	{
		public string Message { get; private set; }

		public ICommand OKCommand { get; }

		public DialogViewModel()
		{
			OKCommand = new RelayCommand(async () => await OK());
		}

		public async Task OK()
		{
			await TryCloseAsync();
		}

		public void UpdateMessage(string message)
		{
			Message = message;
		}
	}
}
