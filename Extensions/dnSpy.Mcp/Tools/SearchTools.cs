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
using dnlib.DotNet.Emit;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for searching within assemblies
/// </summary>
[McpServerToolType]
public sealed class SearchTools {
	readonly DnSpyServices services;

	public SearchTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("Search for string literals in method bodies")]
	public string SearchStrings(
		[Description("Search pattern (supports * wildcard)")] string pattern,
		[Description("Assembly name to search in (optional, searches all if omitted)")] string? assemblyName = null,
		[Description("Case sensitive search (default: false)")] bool caseSensitive = false,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<StringSearchResult>();
		var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		var isWildcard = pattern.Contains('*');

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						if (instr.OpCode.Code == Code.Ldstr && instr.Operand is string str) {
							if (MatchesPattern(str, pattern, comparison, isWildcard)) {
								results.Add(new StringSearchResult {
									Value = str,
									TypeName = type.FullName,
									MethodName = method.Name.String,
									ILOffset = $"IL_{instr.Offset:X4}",
									Assembly = asmName
								});

								if (results.Count >= maxResults)
									return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
							}
						}
					}
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Search for numeric constants in method bodies")]
	public string SearchNumbers(
		[Description("Number to search for")] long value,
		[Description("Assembly name to search in (optional, searches all if omitted)")] string? assemblyName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<NumberSearchResult>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						var (found, foundValue, valueType) = GetNumericOperand(instr);
						if (found && foundValue == value) {
							results.Add(new NumberSearchResult {
								Value = value.ToString(),
								ValueType = valueType,
								TypeName = type.FullName,
								MethodName = method.Name.String,
								ILOffset = $"IL_{instr.Offset:X4}",
								Assembly = asmName
							});

							if (results.Count >= maxResults)
								return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
						}
					}
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	static bool MatchesAssembly(Contracts.Documents.IDsDocument doc, string name) {
		return doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
			(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false);
	}

	static bool MatchesPattern(string value, string pattern, StringComparison comparison, bool isWildcard) {
		if (!isWildcard)
			return value.Contains(pattern, comparison);

		var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
		var options = comparison == StringComparison.OrdinalIgnoreCase
			? System.Text.RegularExpressions.RegexOptions.IgnoreCase
			: System.Text.RegularExpressions.RegexOptions.None;
		return System.Text.RegularExpressions.Regex.IsMatch(value, regex, options);
	}

	static (bool found, long value, string type) GetNumericOperand(Instruction instr) {
		return instr.OpCode.Code switch {
			Code.Ldc_I4_M1 => (true, -1, "int"),
			Code.Ldc_I4_0 => (true, 0, "int"),
			Code.Ldc_I4_1 => (true, 1, "int"),
			Code.Ldc_I4_2 => (true, 2, "int"),
			Code.Ldc_I4_3 => (true, 3, "int"),
			Code.Ldc_I4_4 => (true, 4, "int"),
			Code.Ldc_I4_5 => (true, 5, "int"),
			Code.Ldc_I4_6 => (true, 6, "int"),
			Code.Ldc_I4_7 => (true, 7, "int"),
			Code.Ldc_I4_8 => (true, 8, "int"),
			Code.Ldc_I4_S when instr.Operand is sbyte sb => (true, sb, "sbyte"),
			Code.Ldc_I4 when instr.Operand is int i => (true, i, "int"),
			Code.Ldc_I8 when instr.Operand is long l => (true, l, "long"),
			_ => (false, 0, "")
		};
	}
}

