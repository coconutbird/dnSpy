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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Settings.Dialog;

namespace dnSpy.Mcp;

[Export(typeof(IAppSettingsPageProvider))]
sealed class McpSettingsPageProvider : IAppSettingsPageProvider {
	readonly McpSettingsImpl mcpSettings;

	[ImportingConstructor]
	McpSettingsPageProvider(McpSettingsImpl mcpSettings) => this.mcpSettings = mcpSettings;

	public IEnumerable<AppSettingsPage> Create() {
		yield return new McpAppSettingsPage(mcpSettings);
	}
}

sealed class McpAppSettingsPage : AppSettingsPage {
	readonly McpSettingsImpl mcpSettings;
	int port;
	bool autoStart;

	public override Guid ParentGuid => Guid.Empty;
	public override Guid Guid => new Guid("E8F3A7B2-5C91-4D8E-9F3A-2D7C5E9B1A5F");
	public override double Order => 12000; // After ORDER_BOOKMARKS (11000)
	public override string Title => "MCP Server";
	public override object? UIObject => this;

	public int Port {
		get => port;
		set {
			if (value < 1 || value > 65535)
				value = McpSettings.DefaultPort;
			port = value;
		}
	}

	public bool AutoStart {
		get => autoStart;
		set => autoStart = value;
	}

	public McpAppSettingsPage(McpSettingsImpl mcpSettings) {
		this.mcpSettings = mcpSettings;
		port = mcpSettings.Port;
		autoStart = mcpSettings.AutoStart;
	}

	public override void OnApply() {
		mcpSettings.Port = port;
		mcpSettings.AutoStart = autoStart;
	}
}

