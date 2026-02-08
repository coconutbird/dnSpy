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
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Mcp.Tools;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
#endif

namespace dnSpy.Mcp;

/// <summary>
/// Interface for the MCP server service
/// </summary>
public interface IMcpServerService {
	/// <summary>
	/// Whether the MCP server is currently running
	/// </summary>
	bool IsRunning { get; }

	/// <summary>
	/// The port the MCP server is listening on (0 if not running)
	/// </summary>
	int Port { get; }

	/// <summary>
	/// Start the MCP server
	/// </summary>
	Task StartAsync();

	/// <summary>
	/// Stop the MCP server
	/// </summary>
	Task StopAsync();
}

/// <summary>
/// Service that manages the MCP server lifecycle and provides dnSpy services to MCP tools
/// </summary>
[Export(typeof(IMcpServerService))]
sealed class McpServerService : IMcpServerService {
	readonly IDsDocumentService documentService;
	readonly IDocumentTabService documentTabService;
	readonly IDecompilerService decompilerService;
	readonly IAppWindow appWindow;
	readonly IMcpSettings mcpSettings;

	// Use object to avoid MEF trying to load WebApplication type during part discovery
	object? app;
	CancellationTokenSource? cts;
	int port;

	public bool IsRunning => app is not null;
	public int Port => port;

	[ImportingConstructor]
	McpServerService(
		IDocumentTabService documentTabService,
		IDecompilerService decompilerService,
		IAppWindow appWindow,
		IMcpSettings mcpSettings) {
		this.documentTabService = documentTabService;
		this.documentService = documentTabService.DocumentTreeView.DocumentService;
		this.decompilerService = decompilerService;
		this.appWindow = appWindow;
		this.mcpSettings = mcpSettings;
	}

	public async Task StartAsync() {
#if NETFRAMEWORK
		throw new PlatformNotSupportedException("MCP Server is only supported on .NET 10+. Please use the .NET 10 version of dnSpy.");
#else
		if (app is not null)
			return;

		cts = new CancellationTokenSource();

		// Use the configured port from settings
		port = mcpSettings.Port;

		// Capture dnSpy services for the MCP tools
		var dnSpyServices = new DnSpyServices(
			documentService,
			documentTabService,
			decompilerService,
			appWindow);

		var builder = WebApplication.CreateBuilder();

		// Configure Kestrel to listen on the selected port
		builder.WebHost.UseKestrel(options => {
			options.ListenLocalhost(port);
		});

		// Disable unnecessary logging
		builder.Logging.ClearProviders();
		builder.Logging.SetMinimumLevel(LogLevel.Warning);

		// Register dnSpy services as singleton for DI
		builder.Services.AddSingleton(dnSpyServices);

		// Configure MCP server with HTTP transport
		builder.Services
			.AddMcpServer(options => {
				options.ServerInfo = new() {
					Name = "dnSpy MCP Server",
					Version = "1.0.0"
				};
			})
			.WithHttpTransport()
			.WithTools<DocumentTools>()
			.WithTools<DecompilerTools>()
			.WithTools<AssemblyTools>()
			.WithTools<MemberTools>();

		var webApp = builder.Build();
		app = webApp;  // Store as object to avoid MEF type scanning issues

		// Map MCP endpoints at root path
		webApp.MapMcp("/");

		await webApp.StartAsync(cts.Token);
#endif
	}

	public async Task StopAsync() {
#if NETFRAMEWORK
		await Task.CompletedTask;
#else
		if (app is null)
			return;

		cts?.Cancel();
		var webApp = (WebApplication)app;

		try {
			await webApp.StopAsync();
		}
		finally {
			await webApp.DisposeAsync();
			app = null;
			cts?.Dispose();
			cts = null;
			port = 0;
		}
#endif
	}
}

/// <summary>
/// Container for dnSpy services that MCP tools need access to
/// </summary>
public sealed class DnSpyServices {
	public IDsDocumentService DocumentService { get; }
	public IDocumentTabService DocumentTabService { get; }
	public IDecompilerService DecompilerService { get; }
	public IAppWindow AppWindow { get; }

	public DnSpyServices(
		IDsDocumentService documentService,
		IDocumentTabService documentTabService,
		IDecompilerService decompilerService,
		IAppWindow appWindow) {
		DocumentService = documentService;
		DocumentTabService = documentTabService;
		DecompilerService = decompilerService;
		AppWindow = appWindow;
	}
}

