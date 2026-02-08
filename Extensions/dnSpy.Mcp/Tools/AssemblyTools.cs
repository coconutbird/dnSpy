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
using dnlib.PE;
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;
using File = System.IO.File;
using Path = System.IO.Path;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for assembly metadata and information
/// </summary>
[McpServerToolType]
public sealed class AssemblyTools {
	readonly DnSpyServices services;

	public AssemblyTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("Get comprehensive assembly metadata including version, architecture, entry point, and modules")]
	public string GetAssemblyInfo(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.AssemblyDef is null || doc.ModuleDef is null)
				continue;

			if (!MatchesAssemblyName(doc, assemblyName))
				continue;

			var asm = doc.AssemblyDef;
			var manifestModule = doc.ModuleDef;

			var modules = asm.Modules.Select(mod => new ModuleInfo {
				Name = mod.Name?.String,
				Mvid = mod.Mvid?.ToString(),
				TypeCount = mod.Types.Count,
				IsManifestModule = mod == manifestModule,
				RuntimeVersion = mod.RuntimeVersion,
				Location = mod.Location
			}).ToList();

			var info = new AssemblyInfo {
				Name = asm.Name.String,
				Version = asm.Version.ToString(),
				Culture = string.IsNullOrEmpty(asm.Culture) ? "neutral" : (string)asm.Culture,
				PublicKeyToken = GetPublicKeyToken(asm),
				FullName = asm.FullName,
				TargetFramework = GetTargetFramework(manifestModule),
				Architecture = GetArchitecture(manifestModule),
				EntryPoint = manifestModule.EntryPoint?.FullName,
				IsDebug = HasDebuggableAttribute(asm),
				IsSigned = asm.PublicKey is not null && asm.PublicKey.Data.Length > 0,
				FilePath = doc.Filename,
				CustomAttributeCount = asm.CustomAttributes.Count,
				Modules = modules
			};

			return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });
	}

	[McpServerTool, Description("List all referenced assemblies (dependencies) of an assembly")]
	public string ListAssemblyReferences(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (!MatchesAssemblyName(doc, assemblyName))
				continue;

			var refs = doc.ModuleDef.GetAssemblyRefs().Select(r => new AssemblyReference {
				Name = r.Name.String,
				Version = r.Version.ToString(),
				Culture = string.IsNullOrEmpty(r.Culture) ? "neutral" : (string)r.Culture,
				PublicKeyToken = r.PublicKeyOrToken?.Token?.ToString() ?? "null",
				FullName = r.FullName
			}).ToList();

			return JsonSerializer.Serialize(refs, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });
	}

	[McpServerTool, Description("List custom attributes on an assembly")]
	public string ListAssemblyAttributes(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.AssemblyDef is null)
				continue;

			if (!MatchesAssemblyName(doc, assemblyName))
				continue;

			var attrs = doc.AssemblyDef.CustomAttributes.Select(a => new AssemblyAttribute {
				AttributeType = a.AttributeType?.FullName,
				Arguments = a.ConstructorArguments.Select(arg => new AttributeArgument {
					Type = arg.Type?.FullName,
					Value = arg.Value?.ToString()
				}).ToList()
			}).ToList();

			return JsonSerializer.Serialize(attrs, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });
	}

	[McpServerTool, Description("List all namespaces in an assembly")]
	public string ListNamespaces(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (!MatchesAssemblyName(doc, assemblyName))
				continue;

			var namespaces = doc.ModuleDef.Types
				.Select(t => t.Namespace.String)
				.Where(ns => !string.IsNullOrEmpty(ns))
				.Distinct()
				.OrderBy(ns => ns)
				.Select(ns => new NamespaceInfo {
					Namespace = ns,
					TypeCount = doc.ModuleDef.Types.Count(t => t.Namespace.String == ns)
				})
				.ToList();

			return JsonSerializer.Serialize(namespaces, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });
	}

	bool MatchesAssemblyName(IDsDocument doc, string name) {
		return doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
			(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false);
	}

	static string GetPublicKeyToken(AssemblyDef asm) {
		var token = asm.PublicKeyToken;
		if (token is null || token.Data.Length == 0)
			return "null";
		return string.Join("", token.Data.Select(b => b.ToString("x2")));
	}

	static string GetTargetFramework(ModuleDef mod) {
		var attr = mod.Assembly?.CustomAttributes
			.FirstOrDefault(a => a.AttributeType?.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute");
		if (attr?.ConstructorArguments.Count > 0)
			return attr.ConstructorArguments[0].Value?.ToString() ?? "Unknown";
		return mod.RuntimeVersion ?? "Unknown";
	}

	static string GetArchitecture(ModuleDef mod) {
		if (mod.Machine == Machine.AMD64)
			return "x64";
		if (mod.Machine == Machine.I386) {
			// ComImageFlags.ILOnly = 1, ComImageFlags.Bit32Required = 2
			if ((mod.Cor20HeaderFlags & (dnlib.DotNet.MD.ComImageFlags)1) != 0 &&
				(mod.Cor20HeaderFlags & (dnlib.DotNet.MD.ComImageFlags)2) == 0)
				return "AnyCPU";
			return "x86";
		}
		if (mod.Machine == Machine.ARM64)
			return "ARM64";
		return mod.Machine.ToString();
	}

	static bool HasDebuggableAttribute(AssemblyDef asm) {
		return asm.CustomAttributes.Any(a =>
			a.AttributeType?.FullName == "System.Diagnostics.DebuggableAttribute");
	}

	[McpServerTool, Description("Load an assembly from a file path")]
	public string LoadAssembly(
		[Description("Full path to the assembly file")] string filePath) {
		if (!File.Exists(filePath))
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"File not found: '{filePath}'" });

		try {
			var doc = services.DocumentService.TryGetOrCreate(
				DsDocumentInfo.CreateDocument(filePath),
				isAutoLoaded: false);

			if (doc is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Failed to load assembly from '{filePath}'" });

			return JsonSerializer.Serialize(new {
				Success = true,
				AssemblyName = doc.AssemblyDef?.FullName ?? Path.GetFileName(filePath),
				FilePath = doc.Filename
			}, new JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex) {
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Failed to load assembly: {ex.Message}" });
		}
	}

	[McpServerTool, Description("Unload an assembly from dnSpy")]
	public string UnloadAssembly(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var doc = services.DocumentService.GetDocuments().FirstOrDefault(d => MatchesAssemblyName(d, assemblyName));

		if (doc is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var filename = doc.Filename;
		var asmName = doc.AssemblyDef?.FullName ?? Path.GetFileName(filename);

		services.DocumentService.Remove(new[] { doc });

		return JsonSerializer.Serialize(new {
			Success = true,
			AssemblyName = asmName,
			Message = "Assembly unloaded successfully"
		}, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Reload an assembly from disk")]
	public string ReloadAssembly(
		[Description("Assembly name (partial match supported)")] string assemblyName) {
		var doc = services.DocumentService.GetDocuments().FirstOrDefault(d => MatchesAssemblyName(d, assemblyName));

		if (doc is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var filename = doc.Filename;
		if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly file not found on disk" });

		// Remove and re-add
		services.DocumentService.Remove(new[] { doc });

		try {
			var newDoc = services.DocumentService.TryGetOrCreate(
				DsDocumentInfo.CreateDocument(filename),
				isAutoLoaded: false);

			if (newDoc is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Failed to reload assembly" });

			return JsonSerializer.Serialize(new {
				Success = true,
				AssemblyName = newDoc.AssemblyDef?.FullName ?? Path.GetFileName(filename),
				FilePath = newDoc.Filename,
				Message = "Assembly reloaded successfully"
			}, new JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex) {
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Failed to reload assembly: {ex.Message}" });
		}
	}

	[McpServerTool, Description("Save an assembly to a file (if modified)")]
	public string SaveAssembly(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Output file path (optional, overwrites original if omitted)")] string? outputPath = null) {
		var doc = services.DocumentService.GetDocuments().FirstOrDefault(d => MatchesAssemblyName(d, assemblyName));

		if (doc is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		if (doc.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly has no module definition (may not be a .NET assembly)" });

		var targetPath = outputPath ?? doc.Filename;
		if (string.IsNullOrEmpty(targetPath))
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"No output path specified and assembly has no original path" });

		try {
			doc.ModuleDef.Write(targetPath);

			return JsonSerializer.Serialize(new {
				Success = true,
				AssemblyName = doc.AssemblyDef?.FullName ?? Path.GetFileName(targetPath),
				FilePath = targetPath,
				Message = "Assembly saved successfully"
			}, new JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex) {
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Failed to save assembly: {ex.Message}" });
		}
	}

	[McpServerTool, Description("Get detailed module-level metadata")]
	public string GetModuleInfo(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Module name (optional, defaults to main module)")] string? moduleName = null) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (!MatchesAssemblyName(doc, assemblyName))
				continue;

			if (doc.ModuleDef is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' has no module definition" });

			ModuleDef? targetModule = null;

			if (string.IsNullOrEmpty(moduleName)) {
				// Use main module
				targetModule = doc.ModuleDef;
			}
			else {
				// Find specific module
				if (doc.AssemblyDef is not null) {
					targetModule = doc.AssemblyDef.Modules.FirstOrDefault(m =>
						m.Name.String.Contains(moduleName, StringComparison.OrdinalIgnoreCase));
				}
				if (targetModule is null)
					return JsonSerializer.Serialize(new ErrorResponse { Error = $"Module '{moduleName}' not found in assembly '{assemblyName}'" });
			}

			var info = new {
				Name = targetModule.Name.String,
				Mvid = targetModule.Mvid.ToString(),
				Generation = targetModule.Generation,
				RuntimeVersion = targetModule.RuntimeVersion,
				Kind = targetModule.Kind.ToString(),
				Characteristics = targetModule.Characteristics.ToString(),
				Machine = targetModule.Machine.ToString(),
				CorLibTypes = targetModule.CorLibTypes?.AssemblyRef?.FullName,
				IsManifestModule = doc.AssemblyDef?.ManifestModule == targetModule,
				TypeCount = targetModule.Types.Count,
				ExportedTypeCount = targetModule.ExportedTypes.Count,
				CustomAttributeCount = targetModule.CustomAttributes.Count,
				HasResources = targetModule.Resources.Count > 0,
				ResourceCount = targetModule.Resources.Count,
				EntryPoint = targetModule.EntryPoint?.FullName,
				Location = targetModule.Location
			};

			return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });
	}
}
