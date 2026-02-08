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
using dnSpy.Contracts.MVVM;
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

/// <summary>
/// ViewModel for the MCP settings page UI
/// </summary>
sealed class McpSettingsPageVM : ViewModelBase {
	public int Port {
		get => port;
		set {
			if (value < 1 || value > 65535)
				value = McpSettings.DefaultPort;
			if (port != value) {
				port = value;
				OnPropertyChanged(nameof(Port));
			}
		}
	}

	int port = McpSettings.DefaultPort;

	public bool AutoStart {
		get => autoStart;
		set {
			if (autoStart != value) {
				autoStart = value;
				OnPropertyChanged(nameof(AutoStart));
			}
		}
	}

	bool autoStart;

	public McpSettingsPageVM Clone() => CopyTo(new McpSettingsPageVM());

	public McpSettingsPageVM CopyTo(McpSettingsPageVM other) {
		other.Port = Port;
		other.AutoStart = AutoStart;
		return other;
	}

	public void CopyFrom(McpSettings settings) {
		Port = settings.Port;
		AutoStart = settings.AutoStart;
	}

	public void CopyTo(McpSettings settings) {
		settings.Port = Port;
		settings.AutoStart = AutoStart;
	}
}

sealed class McpAppSettingsPage : AppSettingsPage {
	readonly McpSettingsImpl globalSettings;
	readonly McpSettingsPageVM settings;

	public override Guid ParentGuid => Guid.Empty;
	public override Guid Guid => new Guid("E8F3A7B2-5C91-4D8E-9F3A-2D7C5E9B1A5F");
	public override double Order => AppSettingsConstants.ORDER_MCP;
	public override string Title => "MCP Server";

	public override object? UIObject {
		get {
			if (uiObject is null) {
				uiObject = new McpSettingsControl();
				uiObject.DataContext = settings;
			}
			return uiObject;
		}
	}

	McpSettingsControl? uiObject;

	public McpAppSettingsPage(McpSettingsImpl mcpSettings) {
		globalSettings = mcpSettings;
		settings = new McpSettingsPageVM();
		settings.CopyFrom(mcpSettings);
	}

	public override void OnApply() => settings.CopyTo(globalSettings);
}
