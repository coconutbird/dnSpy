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
using System.ComponentModel;
using System.ComponentModel.Composition;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Settings;

namespace dnSpy.Mcp;

/// <summary>
/// Interface for MCP server settings
/// </summary>
public interface IMcpSettings : INotifyPropertyChanged {
	/// <summary>
	/// The port number for the MCP server (default: 5100)
	/// </summary>
	int Port { get; set; }

	/// <summary>
	/// Whether to auto-start the MCP server when dnSpy starts
	/// </summary>
	bool AutoStart { get; set; }
}

class McpSettings : ViewModelBase, IMcpSettings {
	public const int DefaultPort = 5100;

	public int Port {
		get => port;
		set {
			if (value < 1 || value > 65535)
				value = DefaultPort;
			if (port != value) {
				port = value;
				OnPropertyChanged(nameof(Port));
			}
		}
	}
	int port = DefaultPort;

	public bool AutoStart {
		get => autoStart;
		set {
			if (autoStart != value) {
				autoStart = value;
				OnPropertyChanged(nameof(AutoStart));
			}
		}
	}
	bool autoStart = false;
}

[Export, Export(typeof(IMcpSettings))]
sealed class McpSettingsImpl : McpSettings {
	static readonly Guid SETTINGS_GUID = new Guid("E8F3A7B2-5C91-4D8E-9F3A-2D7C5E9B1A4F");

	readonly ISettingsService settingsService;

	[ImportingConstructor]
	McpSettingsImpl(ISettingsService settingsService) {
		this.settingsService = settingsService;

		var sect = settingsService.GetOrCreateSection(SETTINGS_GUID);
		Port = sect.Attribute<int?>(nameof(Port)) ?? Port;
		AutoStart = sect.Attribute<bool?>(nameof(AutoStart)) ?? AutoStart;
		PropertyChanged += McpSettingsImpl_PropertyChanged;
	}

	void McpSettingsImpl_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
		var sect = settingsService.RecreateSection(SETTINGS_GUID);
		sect.Attribute(nameof(Port), Port);
		sect.Attribute(nameof(AutoStart), AutoStart);
	}
}

