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
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for working with loaded documents/assemblies in dnSpy
/// </summary>
[McpServerToolType]
public sealed class DocumentTools {
	readonly DnSpyServices services;

	public DocumentTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("List all loaded assemblies in dnSpy")]
	public string ListAssemblies() {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<AssemblyListItem>();

		foreach (var doc in docs) {
			string kind;
			if (doc.AssemblyDef is not null)
				kind = "dotnet";
			else if (doc.PEImage is not null)
				kind = "native";
			else
				kind = "other";

			results.Add(new AssemblyListItem {
				Filename = doc.Filename,
				ShortName = doc.GetShortName(),
				Kind = kind,
				IsManaged = doc.AssemblyDef is not null,
				AssemblyName = doc.AssemblyDef?.FullName,
				ModuleName = doc.ModuleDef?.Name?.String,
				ModuleCount = doc.AssemblyDef?.Modules.Count ?? 0
			});
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get types defined in a module/assembly")]
	public string ListTypes(
		[Description("Assembly or module name (partial match supported)")] string assemblyName,
		[Description("Namespace filter (optional)")] string? namespaceFilter = null,
		[Description("Maximum number of results (default: 100)")] int maxResults = 100) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<TypeListItem>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (!doc.Filename.Contains(assemblyName, StringComparison.OrdinalIgnoreCase) &&
				!(doc.AssemblyDef?.Name?.String?.Contains(assemblyName, StringComparison.OrdinalIgnoreCase) ?? false))
				continue;

			foreach (var type in doc.ModuleDef.Types) {
				if (namespaceFilter is not null &&
					!type.Namespace.String.Contains(namespaceFilter, StringComparison.OrdinalIgnoreCase))
					continue;

				results.Add(new TypeListItem {
					FullName = type.FullName,
					Namespace = type.Namespace.String,
					Name = type.Name.String,
					IsPublic = type.IsPublic,
					IsClass = type.IsClass,
					IsInterface = type.IsInterface,
					IsEnum = type.IsEnum,
					IsValueType = type.IsValueType,
					BaseType = type.BaseType?.FullName,
					MethodCount = type.Methods.Count,
					FieldCount = type.Fields.Count,
					PropertyCount = type.Properties.Count
				});

				if (results.Count >= maxResults)
					break;
			}

			if (results.Count >= maxResults)
				break;
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Find types by name pattern across all loaded assemblies")]
	public string FindTypes(
		[Description("Type name pattern to search for")] string pattern,
		[Description("Maximum number of results (default: 50)")] int maxResults = 50) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<TypeSearchResult>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				if (type.Name.String.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
					type.FullName.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
					results.Add(new TypeSearchResult {
						FullName = type.FullName,
						Assembly = doc.GetShortName(),
						IsPublic = type.IsPublic || type.IsNestedPublic,
						Kind = GetTypeKind(type)
					});

					if (results.Count >= maxResults)
						break;
				}
			}

			if (results.Count >= maxResults)
				break;
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific type")]
	public string GetTypeInfo(
		[Description("Full type name (e.g., System.String or MyNamespace.MyClass)")] string typeName) {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			var type = doc.ModuleDef.Find(typeName, true);
			if (type is null)
				continue;

			var info = new TypeInfo {
				FullName = type.FullName,
				Namespace = type.Namespace.String,
				Name = type.Name.String,
				Assembly = doc.GetShortName(),
				Kind = GetTypeKind(type),
				BaseType = type.BaseType?.FullName,
				Interfaces = type.Interfaces.Select(i => i.Interface.FullName).ToList(),
				IsAbstract = type.IsAbstract,
				IsSealed = type.IsSealed,
				IsPublic = type.IsPublic,
				GenericParameters = type.GenericParameters.Select(g => g.Name.String).ToList(),
				Methods = type.Methods.Select(m => new TypeMethodSummary {
					Name = m.Name.String,
					ReturnType = m.ReturnType?.FullName,
					Parameters = m.Parameters.Where(p => !p.IsHiddenThisParameter)
						.Select(p => $"{p.Type?.FullName} {p.Name}").ToList(),
					IsPublic = m.IsPublic,
					IsStatic = m.IsStatic,
					IsConstructor = m.IsConstructor
				}).ToList(),
				Fields = type.Fields.Select(f => new TypeFieldSummary {
					Name = f.Name.String,
					Type = f.FieldType?.FullName,
					IsPublic = f.IsPublic,
					IsStatic = f.IsStatic
				}).ToList(),
				Properties = type.Properties.Select(p => new TypePropertySummary {
					Name = p.Name.String,
					Type = p.PropertySig?.RetType?.FullName,
					HasGetter = p.GetMethod is not null,
					HasSetter = p.SetMethod is not null
				}).ToList()
			};

			return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found in loaded assemblies" });
	}

	static string GetTypeKind(TypeDef type) {
		if (type.IsInterface) return "interface";
		if (type.IsEnum) return "enum";
		if (type.IsValueType) return "struct";
		if (type.IsDelegate) return "delegate";
		return "class";
	}
}

