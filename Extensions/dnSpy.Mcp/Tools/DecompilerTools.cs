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
using System.Text;
using System.Text.Json;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for decompiling .NET code
/// </summary>
[McpServerToolType]
public sealed class DecompilerTools {
	readonly DnSpyServices services;

	public DecompilerTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("List available decompiler languages")]
	public string ListDecompilers() {
		var decompilers = services.DecompilerService.AllDecompilers;
		var results = decompilers.Select(d => new DecompilerInfo {
			Name = d.UniqueNameUI,
			GenericName = d.GenericNameUI,
			FileExtension = d.FileExtension,
			IsCurrent = d == services.DecompilerService.Decompiler
		}).ToList();

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Decompile a type to source code")]
	public string DecompileType(
		[Description("Full type name (e.g., System.String)")] string typeName,
		[Description("Language: csharp, vb, il (default: csharp)")] string language = "csharp") {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			var type = doc.ModuleDef.Find(typeName, true);
			if (type is null)
				continue;

			var decompiler = FindDecompiler(language);
			if (decompiler is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Language '{language}' not found" });

			var output = new StringBuilderDecompilerOutput();
			var ctx = new DecompilationContext();

			decompiler.Decompile(type, output, ctx);

			return output.ToString();
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });
	}

	[McpServerTool, Description("Decompile a specific method")]
	public string DecompileMethod(
		[Description("Full type name containing the method")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Language: csharp, vb, il (default: csharp)")] string language = "csharp") {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			var type = doc.ModuleDef.Find(typeName, true);
			if (type is null)
				continue;

			var methods = type.Methods.Where(m =>
				m.Name.String.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

			if (methods.Count == 0)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{methodName}' not found in type '{typeName}'" });

			var decompiler = FindDecompiler(language);
			if (decompiler is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Language '{language}' not found" });

			var results = new StringBuilder();
			foreach (var method in methods) {
				var output = new StringBuilderDecompilerOutput();
				var ctx = new DecompilationContext();

				decompiler.Decompile(method, output, ctx);

				if (results.Length > 0)
					results.AppendLine("\n// --- Overload ---\n");
				results.Append(output.ToString());
			}

			return results.ToString();
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });
	}

	[McpServerTool, Description("Find methods by name across all loaded assemblies")]
	public string FindMethods(
		[Description("Method name pattern")] string pattern,
		[Description("Maximum results (default: 50)")] int maxResults = 50) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<MethodSearchResult>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Name.String.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
						results.Add(new MethodSearchResult {
							TypeName = type.FullName,
							MethodName = method.Name.String,
							ReturnType = method.ReturnType?.FullName,
							Parameters = method.Parameters.Where(p => !p.IsHiddenThisParameter)
								.Select(p => $"{p.Type?.FullName} {p.Name}").ToList(),
							IsPublic = method.IsPublic,
							IsStatic = method.IsStatic,
							Assembly = doc.GetShortName()
						});

						if (results.Count >= maxResults)
							break;
					}
				}
				if (results.Count >= maxResults)
					break;
			}
			if (results.Count >= maxResults)
				break;
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Decompile a property (getter and/or setter)")]
	public string DecompileProperty(
		[Description("Full type name")] string typeName,
		[Description("Property name")] string propertyName,
		[Description("Language: csharp, vb, il (default: csharp)")] string language = "csharp") {
		var docs = services.DocumentService.GetDocuments();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			var type = doc.ModuleDef.Find(typeName, true);
			if (type is null)
				continue;

			var property = type.Properties.FirstOrDefault(p =>
				p.Name.String.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

			if (property is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Property '{propertyName}' not found in type '{typeName}'" });

			var decompiler = FindDecompiler(language);
			if (decompiler is null)
				return JsonSerializer.Serialize(new ErrorResponse { Error = $"Language '{language}' not found" });

			var results = new StringBuilder();

			// Decompile property signature
			results.AppendLine($"// Property: {property.FullName}");
			results.AppendLine($"// Type: {property.PropertySig?.RetType?.FullName}");
			results.AppendLine();

			// Decompile getter if exists
			if (property.GetMethod is not null) {
				var output = new StringBuilderDecompilerOutput();
				var ctx = new DecompilationContext();
				decompiler.Decompile(property.GetMethod, output, ctx);
				results.AppendLine("// --- Getter ---");
				results.AppendLine(output.ToString());
			}

			// Decompile setter if exists
			if (property.SetMethod is not null) {
				var output = new StringBuilderDecompilerOutput();
				var ctx = new DecompilationContext();
				decompiler.Decompile(property.SetMethod, output, ctx);
				if (property.GetMethod is not null)
					results.AppendLine();
				results.AppendLine("// --- Setter ---");
				results.AppendLine(output.ToString());
			}

			// Decompile other accessors (rare but possible)
			foreach (var other in property.OtherMethods) {
				var output = new StringBuilderDecompilerOutput();
				var ctx = new DecompilationContext();
				decompiler.Decompile(other, output, ctx);
				results.AppendLine();
				results.AppendLine($"// --- Other accessor: {other.Name} ---");
				results.AppendLine(output.ToString());
			}

			return results.ToString();
		}

		return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });
	}

	IDecompiler? FindDecompiler(string language) {
		var lang = language.ToLowerInvariant();
		return services.DecompilerService.AllDecompilers.FirstOrDefault(d =>
			d.UniqueNameUI.Contains(lang, StringComparison.OrdinalIgnoreCase) ||
			d.GenericNameUI.Contains(lang, StringComparison.OrdinalIgnoreCase));
	}
}

