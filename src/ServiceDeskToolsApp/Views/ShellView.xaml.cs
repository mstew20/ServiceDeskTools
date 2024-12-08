using Caliburn.Micro;
using ServiceDeskToolsApp.Commands;
using ServiceDeskToolsApp.Events;
using ServiceDeskToolsApp.Menu;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.Views
{
	/// <summary>
	/// Interaction logic for ShellView.xaml
	/// </summary>
	public partial class ShellView : IHandle<UpdateMenuEvent>
    {
        private readonly IMenuSet _menu;

        public ShellView()
        {
            InitializeComponent();
            _menu = IoC.Get<IMenuSet>();
            var events = IoC.Get<IEventAggregator>();
            events.SubscribeOnUIThread(this);
        }

        public Task HandleAsync(UpdateMenuEvent message, CancellationToken cancellationToken)
        {
            var keep = InputBindings[0];
            InputBindings.Clear();
            InputBindings.Add(keep);
            foreach (IMenuItem item in _menu.Items)
            {
                SetupKeyBindings(item);
            }

            return Task.CompletedTask;
        }

        private void SetupKeyBindings(IMenuItem item)
        {
            if (item.Children?.Count != 0)
            {
                foreach (var child in item.Children)
                {
                    if (child.Command is MenuItemCommand command && command.Gesture is not null)
                    {
                        InputBindings.Add(new KeyBinding(command, command.Gesture));
                    }
                    else
                    {
                        SetupKeyBindings(child);
                    }
                }
            }
        }
    }
}
