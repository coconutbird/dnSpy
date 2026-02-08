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
using dnSpy.Contracts.App;
using dnSpy.Contracts.Menus;

namespace dnSpy.Mcp;

static class McpMenuConstants {
	// Group for MCP commands in View menu - place after other windows
	public const string GROUP_APP_MENU_VIEW_MCP = "2000,B8E3A7F2-4C91-4D8E-9F3A-2D7C5E9B1A4F";
}

/// <summary>
/// Menu item to start/stop the MCP server
/// </summary>
[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "_Start MCP Server", Group = McpMenuConstants.GROUP_APP_MENU_VIEW_MCP, Order = 0)]
sealed class StartMcpServerCommand : MenuItemBase {
	readonly Lazy<IMcpServerService> mcpServerService;

	[ImportingConstructor]
	StartMcpServerCommand(Lazy<IMcpServerService> mcpServerService) {
		this.mcpServerService = mcpServerService;
	}

	public override bool IsVisible(IMenuItemContext context) => !mcpServerService.Value.IsRunning;

	public override async void Execute(IMenuItemContext context) {
		try {
			await mcpServerService.Value.StartAsync();
			var port = mcpServerService.Value.Port;
			MsgBox.Instance.Show($"MCP Server started successfully!\n\nEndpoint: http://localhost:{port}/\n\nMCP clients can connect using this URL.");
		}
		catch (Exception ex) {
			MsgBox.Instance.Show($"Failed to start MCP Server: {ex.Message}");
		}
	}
}

/// <summary>
/// Menu item to stop the MCP server
/// </summary>
[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "S_top MCP Server", Group = McpMenuConstants.GROUP_APP_MENU_VIEW_MCP, Order = 10)]
sealed class StopMcpServerCommand : MenuItemBase {
	readonly Lazy<IMcpServerService> mcpServerService;

	[ImportingConstructor]
	StopMcpServerCommand(Lazy<IMcpServerService> mcpServerService) {
		this.mcpServerService = mcpServerService;
	}

	public override bool IsVisible(IMenuItemContext context) => mcpServerService.Value.IsRunning;

	public override async void Execute(IMenuItemContext context) {
		try {
			await mcpServerService.Value.StopAsync();
			MsgBox.Instance.Show("MCP Server stopped.");
		}
		catch (Exception ex) {
			MsgBox.Instance.Show($"Failed to stop MCP Server: {ex.Message}");
		}
	}
}

