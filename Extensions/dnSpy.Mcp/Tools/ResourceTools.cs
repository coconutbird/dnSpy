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

using System.ComponentModel;
using System.Text.Json;
using dnlib.DotNet;
using dnlib.DotNet.Resources;
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for working with .NET assembly resources
/// </summary>
[McpServerToolType]
public sealed class ResourceTools {
	readonly DnSpyServices services;

	public ResourceTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("List all resources in an assembly")]
	public string ListResources(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resources = doc.ModuleDef.Resources.Select(r => new ResourceListItem {
			Name = r.Name.String,
			Type = r.GetType().Name.Replace("Resource", ""),
			Size = GetResourceSize(r),
			IsPublic = r.Attributes == ManifestResourceAttributes.Public,
			IsEmbedded = r is EmbeddedResource,
			IsLinked = r is LinkedResource || r is AssemblyLinkedResource
		}).ToList();

		return JsonSerializer.Serialize(resources, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("List all .resources files (resource sets) in an assembly")]
	public string ListResourceSets(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resourceSets = doc.ModuleDef.Resources
			.OfType<EmbeddedResource>()
			.Where(r => r.Name.String.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
			.Select(r => {
				var entries = TryReadResourceSet(doc.ModuleDef, r);
				return new ResourceSetInfo {
					Name = r.Name.String,
					EntryCount = entries?.Count ?? 0,
					ResourceType = "ResourceSet"
				};
			}).ToList();

		return JsonSerializer.Serialize(resourceSets, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get content of a specific resource")]
	public string GetResource(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Resource name")] string resourceName,
		[Description("Encoding for text content (utf8, base64). Default: utf8")] string encoding = "utf8") {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resource = doc.ModuleDef.Resources.FirstOrDefault(r =>
			r.Name.String.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

		if (resource is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Resource '{resourceName}' not found" });

		if (resource is not EmbeddedResource embedded)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Resource '{resourceName}' is not embedded" });

		var reader = embedded.CreateReader();
		var bytes = reader.ReadBytes((int)reader.Length);

		var content = new ResourceContent {
			Name = resource.Name.String,
			Size = bytes.Length,
			Encoding = encoding.ToLowerInvariant()
		};

		if (encoding.Equals("base64", StringComparison.OrdinalIgnoreCase)) {
			content.Content = Convert.ToBase64String(bytes);
		}
		else {
			try {
				content.Content = System.Text.Encoding.UTF8.GetString(bytes);
			}
			catch {
				content.Content = Convert.ToBase64String(bytes);
				content.Encoding = "base64";
			}
		}

		content.MimeType = GuessMimeType(resourceName);
		return JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific resource")]
	public string GetResourceInfo(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Resource name")] string resourceName) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resource = doc.ModuleDef.Resources.FirstOrDefault(r =>
			r.Name.String.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

		if (resource is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Resource '{resourceName}' not found" });

		var info = new ResourceInfo {
			Name = resource.Name.String,
			Type = resource.GetType().Name,
			Size = GetResourceSize(resource),
			IsPublic = resource.Attributes == ManifestResourceAttributes.Public,
			ResourceKind = GetResourceKind(resource)
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("List entries in a .resources file (resource set)")]
	public string ListResourceEntries(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Resource set name (e.g., 'MyApp.Properties.Resources.resources')")] string resourceSetName) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resource = doc.ModuleDef.Resources
			.OfType<EmbeddedResource>()
			.FirstOrDefault(r => r.Name.String.Equals(resourceSetName, StringComparison.OrdinalIgnoreCase));

		if (resource is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Resource set '{resourceSetName}' not found" });

		var entries = TryReadResourceSet(doc.ModuleDef, resource);
		if (entries is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Could not read resource set '{resourceSetName}'" });

		var result = entries.Select(e => new ResourceEntry {
			Key = e.Name ?? "",
			Type = e.ResourceData?.GetType().Name ?? "Unknown",
			Size = (int)(e.ResourceData?.EndOffset - e.ResourceData?.StartOffset ?? 0)
		}).ToList();

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get a specific entry from a .resources file")]
	public string GetResourceEntry(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Resource set name (e.g., 'MyApp.Properties.Resources.resources')")] string resourceSetName,
		[Description("Entry key name")] string entryKey) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var resource = doc.ModuleDef.Resources
			.OfType<EmbeddedResource>()
			.FirstOrDefault(r => r.Name.String.Equals(resourceSetName, StringComparison.OrdinalIgnoreCase));

		if (resource is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Resource set '{resourceSetName}' not found" });

		var entries = TryReadResourceSet(doc.ModuleDef, resource);
		if (entries is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Could not read resource set '{resourceSetName}'" });

		var entry = entries.FirstOrDefault(e => e.Name?.Equals(entryKey, StringComparison.OrdinalIgnoreCase) == true);
		if (entry is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Entry '{entryKey}' not found in resource set" });

		var result = new ResourceEntry {
			Key = entry.Name ?? "",
			Type = entry.ResourceData?.GetType().Name ?? "Unknown",
			Size = (int)(entry.ResourceData?.EndOffset - entry.ResourceData?.StartOffset ?? 0),
			Value = GetResourceEntryValue(entry)
		};

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	IDsDocument? FindDocument(string name) {
		return services.DocumentService.GetDocuments().FirstOrDefault(doc =>
			doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
			(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false));
	}

	static int GetResourceSize(Resource r) {
		if (r is EmbeddedResource embedded) {
			try {
				return (int)embedded.CreateReader().Length;
			}
			catch {
				return 0;
			}
		}
		return 0;
	}

	static string GetResourceKind(Resource r) => r switch {
		EmbeddedResource => "Embedded",
		LinkedResource => "Linked",
		AssemblyLinkedResource => "AssemblyLinked",
		_ => "Unknown"
	};

	static List<ResourceElement>? TryReadResourceSet(ModuleDef module, EmbeddedResource resource) {
		try {
			var set = ResourceReader.Read(module, resource.CreateReader());
			return set.ResourceElements.ToList();
		}
		catch {
			return null;
		}
	}

	static string? GetResourceEntryValue(ResourceElement entry) {
		if (entry.ResourceData is BuiltInResourceData builtin) {
			return builtin.Data?.ToString();
		}
		return null;
	}

	static string? GuessMimeType(string name) {
		var ext = Path.GetExtension(name).ToLowerInvariant();
		return ext switch {
			".txt" => "text/plain",
			".xml" => "application/xml",
			".json" => "application/json",
			".png" => "image/png",
			".jpg" or ".jpeg" => "image/jpeg",
			".gif" => "image/gif",
			".ico" => "image/x-icon",
			".bmp" => "image/bmp",
			".xaml" => "application/xaml+xml",
			".baml" => "application/xaml+xml",
			_ => null
		};
	}
}
