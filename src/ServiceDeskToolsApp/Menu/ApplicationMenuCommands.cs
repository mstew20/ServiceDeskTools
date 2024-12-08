using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDeskToolsApp.ViewModels;
using ServiceDeskToolsApp.ViewModels.Tools;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceDeskToolsApp.Menu;
public static partial class ApplicationMenuCommands
{
	private static IConfiguration _config => IoC.Get<IConfiguration>();
	private static IWindowManager _window => IoC.Get<IWindowManager>();
	private static ILogger<App> _logger => IoC.Get<ILogger<App>>();

	public static void CloseApp()
	{
		Application.Current.Shutdown();
	}
	public static async Task NewHiresAsync()
	{
		dynamic settings = new ExpandoObject();
		settings.ResizeMode = ResizeMode.NoResize;

		settings.Title = "New Hires";
		settings.SizeToContent = SizeToContent.Width;
		settings.MaxHeight = 400;
		settings.Height = 400;

		await _window.ShowWindowAsync(IoC.Get<NewHireViewModel>(), null, settings);
	}
	public static async Task OpenSCCM()
	{
		var process = Process.GetProcessesByName("Microsoft.ConfigurationManagement").FirstOrDefault();
		if (process != null)
		{
			BringProcessToFront(process);
		}
		else
		{
			string workingDir = @"C:\Program Files (x86)\Microsoft Configuration Manager\AdminConsole\bin";
			string fileName = $@"{workingDir}\Microsoft.ConfigurationManagement.exe";
			await OpenApplication("SCCM Connect", fileName, workingDir: workingDir);
		}
	}
	public static async Task OpenActiveDirectory()
	{
		await OpenApplication("Open AD", "cmd.exe", "/c dsa.msc", @"C:\Windows\System32");
	}
	public static async Task OpenServices()
	{
		await OpenApplication("Open Services", "cmd.exe", "/c services.msc /computer=", @"C:\Windows\System32");
	}
	public static async Task OpenApplication(string title, string fileName, string arugments = null, string workingDir = "", bool useShell = false)
	{
		var vm = IoC.Get<AccountDialogViewModel>();
		dynamic settings = new ExpandoObject();
		settings.ResizeMode = ResizeMode.NoResize;
		settings.Title = title;
		settings.Height = 200;
		settings.Width = 400;
		settings.SizeToContent = SizeToContent.Manual;

		bool failed;
		do
		{
			failed = false;
			if (await _window.ShowDialogAsync(vm, null, settings) == true)
			{
				ProcessStartInfo psi = new()
				{
					WorkingDirectory = workingDir,
					UseShellExecute = useShell,
					Arguments = arugments,
					CreateNoWindow = true,
					FileName = fileName
				};

				if (!string.IsNullOrWhiteSpace(vm.UserName))
				{
					psi.LoadUserProfile = true;
					psi.UserName = vm.UserName;
					psi.PasswordInClearText = vm.Password;
					psi.Domain = "<domain name>";
				}

				try
				{
					Process.Start(psi);
				}
				catch (Exception ex)
				{
					_logger.LogError("Unable to open application {fileName}\n\t Error:{message}", fileName, ex.Message);
					failed = true;
				}
			}
		} while (failed);
	}
	public static async Task OpenGroupSearch(GroupSearchViewModel vm = null)
	{
		if (vm is null)
		{
			vm = IoC.Get<GroupSearchViewModel>();
		}
		dynamic settings = new ExpandoObject();
		settings.Title = "Search Groups";
		settings.Owner = null;
		settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		await _window.ShowWindowAsync(vm, null, settings);
	}
	public static async Task OpenComputerSearch()
	{
		var vm = IoC.Get<ComputerSearchViewModel>();
		dynamic settings = new ExpandoObject();
		settings.Title = "Search Computers";
		settings.Owner = null;
		settings.Width = 400;
		settings.Height = 300;
		settings.SizeToContent = SizeToContent.Manual;
		settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		await _window.ShowWindowAsync(vm, null, settings);
	}
	public static async Task OpenPing()
	{
		var vm = IoC.Get<PingViewModel>();
		dynamic settings = new ExpandoObject();
		settings.Title = "Ping";
		settings.Width = 569;
		settings.SizeToContent = SizeToContent.Height;
		settings.Owner = null;
		settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		await _window.ShowWindowAsync(vm, null, settings);
	}
	public static async Task OpenLoggedInUser()
	{
		var vm = IoC.Get<LoggedInUserViewModel>();
		dynamic settings = new ExpandoObject();
		settings.Title = "Logged In User";
		settings.Width = 450;
		settings.Height = 300;
		settings.SizeToContent = SizeToContent.Manual;
		settings.Owner = null;
		settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		await _window.ShowWindowAsync(vm, null, settings);
	}
	public static async Task OpenBitLocker()
	{
		var vm = IoC.Get<BitlockerViewModel>();
		dynamic settings = new ExpandoObject();
		settings.Title = "Bitlocker";
		settings.Width = 550;
		settings.Height = 400;
		settings.SizeToContent = SizeToContent.Manual;
		settings.Owner = null;
		settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		await _window.ShowWindowAsync(vm, null, settings);
	}
	public static async Task ManageTools()
	{
		dynamic settings = new ExpandoObject();
		settings.ResizeMode = ResizeMode.CanResize;
		settings.Title = "Manage Tools";
		settings.Height = 400;
		settings.Width = 400;
		settings.SizeToContent = SizeToContent.Manual;
		await _window.ShowDialogAsync(IoC.Get<ManageToolsViewModel>(), null, settings);
	}

	private static void BringProcessToFront(Process process)
	{
		IntPtr handle = process.MainWindowHandle;
		if (IsIconic(handle))
		{
			ShowWindow(handle, SW_RESTORE);
		}

		SetForegroundWindow(handle);
	}
	const int SW_RESTORE = 9;
	[LibraryImport("User32.dll", EntryPoint = "SetForegroundWindow")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetForegroundWindow(IntPtr handle);
	[LibraryImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ShowWindow(IntPtr handle, int nCmdShow);
	[LibraryImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsIconic(IntPtr handle);
}
