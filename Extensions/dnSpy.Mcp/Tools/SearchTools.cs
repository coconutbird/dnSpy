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

	[McpServerTool, Description("Search for regex pattern in decompiled code")]
	public string SearchRegex(
		[Description("Regex pattern to search for")] string pattern,
		[Description("Assembly name to search in (optional)")] string? assemblyName = null,
		[Description("Search scope: all, methods, types (default: all)")] string searchIn = "all",
		[Description("Case sensitive (default: false)")] bool caseSensitive = false,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		System.Text.RegularExpressions.Regex regex;
		try {
			var options = caseSensitive
				? System.Text.RegularExpressions.RegexOptions.None
				: System.Text.RegularExpressions.RegexOptions.IgnoreCase;
			regex = new System.Text.RegularExpressions.Regex(pattern, options);
		}
		catch (Exception ex) {
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Invalid regex pattern: {ex.Message}" });
		}

		var results = new List<RegexMatchInfo>();
		var decompiler = services.DecompilerService.Decompiler;

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				if (searchIn == "types" || searchIn == "all") {
					// Search in type name
					if (regex.IsMatch(type.FullName)) {
						results.Add(new RegexMatchInfo {
							TypeName = type.FullName,
							MethodName = "",
							LineNumber = 0,
							MatchedLine = type.FullName,
							Assembly = asmName
						});
						if (results.Count >= maxResults) goto done;
					}
				}

				if (searchIn == "methods" || searchIn == "all") {
					foreach (var method in type.Methods) {
						// Decompile method and search
						try {
							var output = new dnSpy.Contracts.Decompiler.StringBuilderDecompilerOutput();
							var ctx = new dnSpy.Contracts.Decompiler.DecompilationContext();
							decompiler.Decompile(method, output, ctx);
							var code = output.ToString();

							var matches = regex.Matches(code);
							if (matches.Count > 0) {
								results.Add(new RegexMatchInfo {
									TypeName = type.FullName,
									MethodName = method.Name.String,
									LineNumber = 1,
									MatchedLine = matches[0].Value.Length > 100
										? matches[0].Value.Substring(0, 100) + "..."
										: matches[0].Value,
									Assembly = asmName
								});
								if (results.Count >= maxResults) goto done;
							}
						}
						catch {
							// Skip methods that can't be decompiled
						}
					}
				}
			}
		}

		done:
		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Search for IL instruction patterns")]
	public string SearchILPattern(
		[Description("IL pattern (e.g., 'call *GetString*' or 'ldstr;call')")] string pattern,
		[Description("Assembly name to search in (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<ILPatternMatchInfo>();
		var patternParts = pattern.Split(';').Select(p => p.Trim().ToLowerInvariant()).ToArray();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					var instructions = method.Body.Instructions;
					for (int i = 0; i <= instructions.Count - patternParts.Length; i++) {
						if (MatchesILPattern(instructions, i, patternParts)) {
							var matchedInstrs = instructions.Skip(i).Take(patternParts.Length)
								.Select(instr => $"IL_{instr.Offset:X4}: {instr.OpCode.Name} {GetOperandString(instr)}")
								.ToList();

							results.Add(new ILPatternMatchInfo {
								TypeName = type.FullName,
								MethodName = method.Name.String,
								ILOffset = $"IL_{instructions[i].Offset:X4}",
								MatchedInstructions = matchedInstrs,
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

	static bool MatchesILPattern(IList<Instruction> instructions, int startIndex, string[] patternParts) {
		for (int j = 0; j < patternParts.Length; j++) {
			var instr = instructions[startIndex + j];
			var part = patternParts[j];

			// Check if pattern contains wildcard
			if (part.Contains('*')) {
				var opcodePart = part.Split(' ')[0];
				var operandPart = part.Contains(' ') ? part.Substring(part.IndexOf(' ') + 1) : null;

				// Match opcode
				if (!MatchesWildcard(instr.OpCode.Name.ToLowerInvariant(), opcodePart))
					return false;

				// Match operand if specified
				if (operandPart is not null) {
					var operandStr = GetOperandString(instr).ToLowerInvariant();
					if (!MatchesWildcard(operandStr, operandPart))
						return false;
				}
			}
			else {
				// Exact match on opcode
				var opcodePart = part.Split(' ')[0];
				if (!instr.OpCode.Name.Equals(opcodePart, StringComparison.OrdinalIgnoreCase))
					return false;

				// Check operand if specified
				if (part.Contains(' ')) {
					var operandPart = part.Substring(part.IndexOf(' ') + 1);
					var operandStr = GetOperandString(instr).ToLowerInvariant();
					if (!operandStr.Contains(operandPart))
						return false;
				}
			}
		}
		return true;
	}

	static bool MatchesWildcard(string value, string pattern) {
		var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
		return System.Text.RegularExpressions.Regex.IsMatch(value, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
	}

	static string GetOperandString(Instruction instr) {
		if (instr.Operand is null) return "";
		if (instr.Operand is string s) return $"\"{s}\"";
		if (instr.Operand is IMethod m) return m.FullName;
		if (instr.Operand is IField f) return f.FullName;
		if (instr.Operand is ITypeDefOrRef t) return t.FullName;
		if (instr.Operand is Instruction target) return $"IL_{target.Offset:X4}";
		return instr.Operand.ToString() ?? "";
	}

	[McpServerTool, Description("Search for types or members with specific attributes")]
	public string SearchAttributes(
		[Description("Attribute type name pattern (e.g., 'Obsolete' or '*Serializable*')")] string attributePattern,
		[Description("Search target: types, methods, fields, properties, all (default: all)")] string searchTarget = "all",
		[Description("Assembly name to search in (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<AttributeMatchInfo>();
		var isWildcard = attributePattern.Contains('*');

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				// Search type attributes
				if (searchTarget == "types" || searchTarget == "all") {
					foreach (var attr in type.CustomAttributes) {
						if (MatchesAttributeName(attr.AttributeType?.Name, attributePattern, isWildcard)) {
							results.Add(new AttributeMatchInfo {
								TargetKind = "Type",
								TargetName = type.FullName,
								AttributeType = attr.AttributeType?.FullName ?? "",
								Assembly = asmName
							});
							if (results.Count >= maxResults) goto done;
						}
					}
				}

				// Search method attributes
				if (searchTarget == "methods" || searchTarget == "all") {
					foreach (var method in type.Methods) {
						foreach (var attr in method.CustomAttributes) {
							if (MatchesAttributeName(attr.AttributeType?.Name, attributePattern, isWildcard)) {
								results.Add(new AttributeMatchInfo {
									TargetKind = "Method",
									TargetName = $"{type.FullName}.{method.Name}",
									AttributeType = attr.AttributeType?.FullName ?? "",
									Assembly = asmName
								});
								if (results.Count >= maxResults) goto done;
							}
						}
					}
				}

				// Search field attributes
				if (searchTarget == "fields" || searchTarget == "all") {
					foreach (var field in type.Fields) {
						foreach (var attr in field.CustomAttributes) {
							if (MatchesAttributeName(attr.AttributeType?.Name, attributePattern, isWildcard)) {
								results.Add(new AttributeMatchInfo {
									TargetKind = "Field",
									TargetName = $"{type.FullName}.{field.Name}",
									AttributeType = attr.AttributeType?.FullName ?? "",
									Assembly = asmName
								});
								if (results.Count >= maxResults) goto done;
							}
						}
					}
				}

				// Search property attributes
				if (searchTarget == "properties" || searchTarget == "all") {
					foreach (var prop in type.Properties) {
						foreach (var attr in prop.CustomAttributes) {
							if (MatchesAttributeName(attr.AttributeType?.Name, attributePattern, isWildcard)) {
								results.Add(new AttributeMatchInfo {
									TargetKind = "Property",
									TargetName = $"{type.FullName}.{prop.Name}",
									AttributeType = attr.AttributeType?.FullName ?? "",
									Assembly = asmName
								});
								if (results.Count >= maxResults) goto done;
							}
						}
					}
				}
			}
		}

		done:
		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	static bool MatchesAttributeName(UTF8String? name, string pattern, bool isWildcard) {
		if (name is null) return false;
		var nameStr = name.String;
		// Also match without "Attribute" suffix
		var patternWithSuffix = pattern.EndsWith("Attribute") ? pattern : pattern + "Attribute";

		if (isWildcard) {
			return MatchesWildcard(nameStr, pattern) || MatchesWildcard(nameStr, patternWithSuffix);
		}
		return nameStr.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
			   nameStr.Contains(patternWithSuffix, StringComparison.OrdinalIgnoreCase);
	}

	[McpServerTool, Description("Search for methods by signature pattern")]
	public string SearchBySignature(
		[Description("Return type pattern (e.g., 'void', 'System.String', '*' for any)")] string? returnType = null,
		[Description("Parameter types pattern (e.g., 'int, string' or 'System.Object, *')")] string? parameterTypes = null,
		[Description("Method name pattern (optional)")] string? methodName = null,
		[Description("Assembly name to search in (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		if (returnType is null && parameterTypes is null && methodName is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = "At least one search criterion must be provided" });

		var results = new List<SignatureMatchInfo>();
		var paramPatterns = parameterTypes?.Split(',').Select(p => p.Trim()).ToArray();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			if (assemblyName is not null && !MatchesAssembly(doc, assemblyName)) continue;

			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					// Check return type
					if (returnType is not null && returnType != "*") {
						var retTypeName = method.ReturnType?.FullName ?? "void";
						if (!MatchesTypePattern(retTypeName, returnType))
							continue;
					}

					// Check method name
					if (methodName is not null) {
						if (!MatchesWildcard(method.Name.String, methodName) &&
							!method.Name.String.Contains(methodName, StringComparison.OrdinalIgnoreCase))
							continue;
					}

					// Check parameter types
					if (paramPatterns is not null) {
						var methodParams = method.Parameters
							.Where(p => !p.IsHiddenThisParameter)
							.Select(p => p.Type?.FullName ?? "?")
							.ToArray();

						if (!MatchesParameterPattern(methodParams, paramPatterns))
							continue;
					}

					results.Add(new SignatureMatchInfo {
						TypeName = type.FullName,
						MethodName = method.Name.String,
						ReturnType = method.ReturnType?.FullName ?? "void",
						Parameters = method.Parameters
							.Where(p => !p.IsHiddenThisParameter)
							.Select(p => new SignatureParameterInfo {
								Type = p.Type?.FullName ?? "?",
								Name = p.Name
							})
							.ToList(),
						FullSignature = method.FullName,
						Assembly = asmName
					});

					if (results.Count >= maxResults)
						return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	static bool MatchesTypePattern(string typeName, string pattern) {
		if (pattern == "*") return true;
		if (pattern.Contains('*'))
			return MatchesWildcard(typeName, pattern);
		return typeName.Contains(pattern, StringComparison.OrdinalIgnoreCase);
	}

	static bool MatchesParameterPattern(string[] methodParams, string[] patterns) {
		if (patterns.Length != methodParams.Length) return false;

		for (int i = 0; i < patterns.Length; i++) {
			if (patterns[i] != "*" && !MatchesTypePattern(methodParams[i], patterns[i]))
				return false;
		}
		return true;
	}
}

