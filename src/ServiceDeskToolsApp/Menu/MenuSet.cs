using ServiceDeskToolsApp.Models;
using ServiceDeskToolsApp.Settings;
using ServiceDeskToolsApp.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.Menu;
public class MenuSet : IMenuSet
{
	private readonly ApplicationSettings _settings;

	public BindingList<IMenuItem> Items { get; set; }

	public MenuSet(ApplicationSettings settings)
	{
		_settings = settings;
		Items = [];
	}

	public async Task AddToolsMenu()
	{
		var tools = new MenuItemViewModel("Tools");
		tools.Children.Add(new MenuItemViewModel("Active Directory", async () => await ApplicationMenuCommands.OpenActiveDirectory(), new(Key.A, ModifierKeys.Control | ModifierKeys.Shift)));
		tools.Children.Add(new MenuItemViewModel("SCCM", async () => await ApplicationMenuCommands.OpenSCCM(), new(Key.S, ModifierKeys.Control)));
		tools.Children.Add(new MenuItemViewModel("Windows Services", async () => await ApplicationMenuCommands.OpenServices(), new(Key.W, ModifierKeys.Control)));
		tools.Children.Add(new MenuItemViewModel("Ping", async () => await ApplicationMenuCommands.OpenPing(), new(Key.P, ModifierKeys.Control)));
		tools.Children.Add(new MenuItemViewModel("Logged in User", async () => await ApplicationMenuCommands.OpenLoggedInUser(), new(Key.L, ModifierKeys.Control)));
		tools.Children.Add(new SeparatorViewModel());

		using var stream = new FileStream($"{_settings.LocalDataFiles}\\tools.json", FileMode.OpenOrCreate);
		if (stream.Length != 0)
		{
			var jsonTools = await JsonSerializer.DeserializeAsync<ToolsWithHotKey[]>(stream);
			foreach (var tool in jsonTools)
			{
				MenuItemViewModel newTool;
				newTool = new(tool.Header, async () => await ApplicationMenuCommands.OpenApplication(tool.Header, tool.FileName, tool.Arguemnts, tool.WorkingDirectory));
				if (tool.HotKey is not null)
				{
					newTool = new(tool.Header, async () => await ApplicationMenuCommands.OpenApplication(tool.Header, tool.FileName, tool.Arguemnts, tool.WorkingDirectory), new(tool.HotKey.Key, tool.HotKey.Modifiers));
				}

				tools.Children.Add(newTool);
			}
		}
		tools.Children.Add(new MenuItemViewModel("Manage Tools...", async () => await ApplicationMenuCommands.ManageTools()));
		Items.Add(tools);
	}
	private void AddSeachMenu()
	{
		var search = new MenuItemViewModel("S_earch");
		search.Children.Add(new MenuItemViewModel("_Computers", async () => await ApplicationMenuCommands.OpenComputerSearch()));
		search.Children.Add(new MenuItemViewModel("_Groups", async () => await ApplicationMenuCommands.OpenGroupSearch()));
		search.Children.Add(new MenuItemViewModel("_Bitlocker", async () => await ApplicationMenuCommands.OpenBitLocker(), new(Key.B, ModifierKeys.Control)));
		Items.Add(search);
	}

	public async Task Build()
	{
		var file = new MenuItemViewModel("_File");
		file.Children.Add(new MenuItemViewModel("E_xit", ApplicationMenuCommands.CloseApp));
		Items.Add(file);

		AddSeachMenu();

		var newHires = new MenuItemViewModel("New Hires", async () => await ApplicationMenuCommands.NewHiresAsync());
		Items.Add(newHires);

		await AddToolsMenu();
	}
}
