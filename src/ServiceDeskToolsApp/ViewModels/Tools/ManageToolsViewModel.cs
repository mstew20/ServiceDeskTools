using Caliburn.Micro;
using ServiceDeskToolsApp.Menu;
using ServiceDeskToolsApp.Models;
using ServiceDeskToolsApp.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels.Tools;
public class ManageToolsViewModel : Screen
{
	private readonly IMenuSet _menu;
	private readonly IWindowManager _window;
	private readonly ApplicationSettings _settings;
	private readonly IEventAggregator _events;
	private IList<IMenuItem> _existingTools;
	private ToolsWithHotKey _selectedTool;
	private List<IMenuItem> _toDelete = [];

	public ObservableCollection<ToolsWithHotKey> Tools { get; set; }
	public ToolsWithHotKey SelectedTool
	{
		get => _selectedTool;
		set
		{
			_selectedTool = value;
			NotifyOfPropertyChange();
		}
	}
	public ICommand EditCommand { get; }


	public ManageToolsViewModel(IMenuSet menu, IWindowManager window, IEventAggregator @event, ApplicationSettings appSettings)
	{
		Tools = [];
		_menu = menu;
		_window = window;
		_settings = appSettings;
		_events = @event;
		_existingTools = _menu.Items.Where(x => x.Header == "Tools").FirstOrDefault().Children;

		EditCommand = new DelegateCommand<ToolsWithHotKey>(async (e) => await Edit(e));
	}

	public async Task Save()
	{
		foreach (var tool in _toDelete)
		{
			_existingTools.Remove(tool);
		}

		int count = _existingTools.Count - 1;
		int pos = 6;
		for (int i = pos; i < count; i++)
		{
			_existingTools.RemoveAt(pos);
		}

		foreach (var tool in Tools)
		{
			MenuItemViewModel newTool;
			newTool = new(tool.Header, async () => await ApplicationMenuCommands.OpenApplication(tool.Header, tool.FileName, tool.Arguemnts, tool.WorkingDirectory));
			if (tool.HotKey is not null)
			{
				newTool = new(tool.Header, async () => await ApplicationMenuCommands.OpenApplication(tool.Header, tool.FileName, tool.Arguemnts, tool.WorkingDirectory), new(tool.HotKey.Key, tool.HotKey.Modifiers));
			}

			_existingTools.Insert(_existingTools.Count - 1, newTool);
		}
		using var stream = new MemoryStream();
		await JsonSerializer.SerializeAsync(stream, Tools.ToArray(), options: new() { WriteIndented = true });
		stream.Position = 0;
		using var file = File.OpenWrite($"{_settings.LocalDataFiles}\\tools.json");
		await stream.CopyToAsync(file);

		await _events.PublishOnUIThreadAsync(new UpdateMenuEvent());
	}
	public async Task Add()
	{
		await AddOrEdit();
	}
	public async Task Edit(ToolsWithHotKey tool)
	{
		await AddOrEdit(tool);
	}
	private async Task AddOrEdit(ToolsWithHotKey toolToEdit = null)
	{
		dynamic settings = new ExpandoObject();
		settings.ResizeMode = ResizeMode.CanResize;
		settings.Title = "Add or edit tool";
		settings.Height = 250;
		settings.Width = 600;
		settings.SizeToContent = SizeToContent.Manual;
		var vm = IoC.Get<AddNewToolViewModel>();

		if (toolToEdit is not null)
		{
			vm.Name = toolToEdit.Header;
			vm.FileName = toolToEdit.FileName;
			vm.Arguemnts = toolToEdit.Arguemnts;
			vm.WorkingDirectory = toolToEdit.WorkingDirectory;
		}

		if (await _window.ShowDialogAsync(vm, null, settings) == true)
		{
			if (toolToEdit is not null)
			{
				toolToEdit.Header = vm.Name;
				toolToEdit.FileName = vm.FileName;
				toolToEdit.Arguemnts = vm.Arguemnts;
				toolToEdit.WorkingDirectory = vm.WorkingDirectory;
			}
			else
			{
				Tools.Add(new()
				{
					Header = vm.Name,
					FileName = vm.FileName,
					Arguemnts = vm.Arguemnts,
					WorkingDirectory = vm.WorkingDirectory
				});
			}
		}
	}
	public void Delete()
	{
		var item = _existingTools.First(x => x.Header == SelectedTool.Header);
		_toDelete.Add(item);
		Tools.Remove(SelectedTool);
	}
	public void MoveUp()
	{
		var index = Tools.IndexOf(SelectedTool);
		if (index > 0)
		{
			Tools.Move(index, index - 1);
		}
	}
	public void MoveDown()
	{
		var index = Tools.IndexOf(SelectedTool);
		if (index < Tools.Count - 1)
		{
			Tools.Move(index, index + 1);
		}
	}

	protected override async void OnViewLoaded(System.Object view)
	{
		using var stream = new FileStream($"{_settings.LocalDataFiles}\\tools.json", FileMode.OpenOrCreate);
		if (stream.Length != 0)
		{
			var tools = await JsonSerializer.DeserializeAsync<ToolsWithHotKey[]>(stream);
			foreach (var tool in tools)
			{
				Tools.Add(tool);
			}
		}
		base.OnViewLoaded(view);
	}
}
