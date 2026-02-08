/*
    Copyright (C) 2014-2019 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.ToolBars;

namespace dnSpy.Mcp;

static class McpMenuConstants {
	// Group for MCP commands in View menu - place after other windows
	public const string GROUP_APP_MENU_VIEW_MCP = "2000,B8E3A7F2-4C91-4D8E-9F3A-2D7C5E9B1A4F";
}

static class McpToolBarConstants {
	// Toolbar group for MCP - place between assembly editor and debugger
	// GROUP_APP_TB_MAIN_ASMED_UNDO = "4000,..." and GROUP_APP_TB_MAIN_DEBUG = "5000,..."
	public const string GROUP_APP_TB_MCP = "4500,C9F3A8B2-6D91-4E8F-9F4A-3E8C6F0B2A5D";
}

/// <summary>
/// Routed command for toggling the MCP server
/// </summary>
static class McpCommands {
	public static readonly RoutedCommand ToggleMcpServer = new RoutedCommand("ToggleMcpServer", typeof(McpCommands));
}

/// <summary>
/// Loader that registers keyboard shortcuts and auto-starts the MCP server if configured
/// </summary>
[ExportAutoLoaded]
sealed class McpCommandLoader : IAutoLoaded {
	readonly Lazy<IMcpServerService> mcpServerService;
	readonly Lazy<IAppWindow> appWindow;

	[ImportingConstructor]
	McpCommandLoader(
		IWpfCommandService wpfCommandService,
		Lazy<IMcpServerService> mcpServerService,
		Lazy<IMcpSettings> mcpSettings,
		Lazy<IAppWindow> appWindow) {
		this.mcpServerService = mcpServerService;
		this.appWindow = appWindow;

		// Register the toggle command with keyboard shortcut Ctrl+Alt+M
		var cmds = wpfCommandService.GetCommands(ControlConstants.GUID_MAINWINDOW);
		cmds.Add(McpCommands.ToggleMcpServer, new RelayCommand(a => ToggleServer()));
		cmds.Add(McpCommands.ToggleMcpServer, ModifierKeys.Control | ModifierKeys.Alt, Key.M);

		// Auto-start the MCP server if configured
		if (mcpSettings.Value.AutoStart) {
			// Use Task.Run to avoid blocking the UI thread during startup
			Task.Run(async () => {
				try {
					await mcpServerService.Value.StartAsync();
					ShowInfoBarOnUIThread();
				}
				catch {
					// Silently fail on auto-start - user can manually start later
				}
			});
		}
	}

	async void ToggleServer() {
		try {
			if (mcpServerService.Value.IsRunning) {
				await mcpServerService.Value.StopAsync();
				appWindow.Value.InfoBar.Show("MCP Server stopped", InfoBarIcon.Information);
			}
			else {
				await mcpServerService.Value.StartAsync();
				ShowInfoBar();
			}
		}
		catch (Exception ex) {
			MsgBox.Instance.Show($"MCP Server error: {ex.Message}");
		}
	}

	void ShowInfoBarOnUIThread() {
		// Marshal to UI thread
		System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(ShowInfoBar));
	}

	void ShowInfoBar() {
		var port = mcpServerService.Value.Port;
		appWindow.Value.InfoBar.Show(
			$"MCP Server running on http://localhost:{port}/",
			InfoBarIcon.Information,
			new InfoBarInteraction("Copy URL", ctx => {
				System.Windows.Clipboard.SetText($"http://localhost:{port}/");
				ctx.CloseElement();
			}),
			new InfoBarInteraction("Stop", async ctx => {
				try {
					await mcpServerService.Value.StopAsync();
				}
				catch { }
				ctx.CloseElement();
			}));
	}
}

/// <summary>
/// Toolbar button to toggle the MCP server on/off
/// </summary>
[ExportToolBarButton(Icon = DsImagesAttribute.DownloadNoColor, ToolTip = "Toggle MCP Server (Ctrl+Alt+M)", IsToggleButton = true, Group = McpToolBarConstants.GROUP_APP_TB_MCP, Order = 0)]
sealed class McpServerToolBarButton : ToolBarButtonBase {
	readonly Lazy<IMcpServerService> mcpServerService;
	readonly Lazy<IAppWindow> appWindow;

	[ImportingConstructor]
	McpServerToolBarButton(Lazy<IMcpServerService> mcpServerService, Lazy<IAppWindow> appWindow) {
		this.mcpServerService = mcpServerService;
		this.appWindow = appWindow;
	}

	public override void Execute(IToolBarItemContext context) {
		// Delegate to the routed command
		if (McpCommands.ToggleMcpServer.CanExecute(null, null))
			McpCommands.ToggleMcpServer.Execute(null, null);
	}

	public override string? GetToolTip(IToolBarItemContext context) {
		if (mcpServerService.Value.IsRunning) {
			var port = mcpServerService.Value.Port;
			return $"MCP Server running on port {port} - Click to stop (Ctrl+Alt+M)";
		}
		return "Start MCP Server (Ctrl+Alt+M)";
	}

	public override ImageReference? GetIcon(IToolBarItemContext context) {
		// Use Run icon when server is running, DownloadNoColor when stopped
		return mcpServerService.Value.IsRunning
			? DsImages.Run
			: DsImages.DownloadNoColor;
	}
}

/// <summary>
/// Menu item to start the MCP server
/// </summary>
[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "_Start MCP Server", Icon = DsImagesAttribute.Run, InputGestureText = "Ctrl+Alt+M", Group = McpMenuConstants.GROUP_APP_MENU_VIEW_MCP, Order = 0)]
sealed class StartMcpServerCommand : MenuItemBase {
	readonly Lazy<IMcpServerService> mcpServerService;
	readonly Lazy<IAppWindow> appWindow;

	[ImportingConstructor]
	StartMcpServerCommand(Lazy<IMcpServerService> mcpServerService, Lazy<IAppWindow> appWindow) {
		this.mcpServerService = mcpServerService;
		this.appWindow = appWindow;
	}

	public override bool IsVisible(IMenuItemContext context) => !mcpServerService.Value.IsRunning;

	public override async void Execute(IMenuItemContext context) {
		try {
			await mcpServerService.Value.StartAsync();
			var port = mcpServerService.Value.Port;
			appWindow.Value.InfoBar.Show(
				$"MCP Server running on http://localhost:{port}/",
				InfoBarIcon.Information,
				new InfoBarInteraction("Copy URL", ctx => {
					System.Windows.Clipboard.SetText($"http://localhost:{port}/");
					ctx.CloseElement();
				}),
				new InfoBarInteraction("Stop", async ctx => {
					try {
						await mcpServerService.Value.StopAsync();
					}
					catch { }
					ctx.CloseElement();
				}));
		}
		catch (Exception ex) {
			MsgBox.Instance.Show($"Failed to start MCP Server: {ex.Message}");
		}
	}
}

/// <summary>
/// Menu item to stop the MCP server
/// </summary>
[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "S_top MCP Server", Icon = DsImagesAttribute.Stop, Group = McpMenuConstants.GROUP_APP_MENU_VIEW_MCP, Order = 10)]
sealed class StopMcpServerCommand : MenuItemBase {
	readonly Lazy<IMcpServerService> mcpServerService;
	readonly Lazy<IAppWindow> appWindow;

	[ImportingConstructor]
	StopMcpServerCommand(Lazy<IMcpServerService> mcpServerService, Lazy<IAppWindow> appWindow) {
		this.mcpServerService = mcpServerService;
		this.appWindow = appWindow;
	}

	public override bool IsVisible(IMenuItemContext context) => mcpServerService.Value.IsRunning;

	public override async void Execute(IMenuItemContext context) {
		try {
			await mcpServerService.Value.StopAsync();
			appWindow.Value.InfoBar.Show("MCP Server stopped", InfoBarIcon.Information);
		}
		catch (Exception ex) {
			MsgBox.Instance.Show($"Failed to stop MCP Server: {ex.Message}");
		}
	}
}

