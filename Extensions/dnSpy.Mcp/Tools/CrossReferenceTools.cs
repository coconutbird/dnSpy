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
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for cross-reference analysis
/// </summary>
[McpServerToolType]
public sealed class CrossReferenceTools {
	readonly DnSpyServices services;

	public CrossReferenceTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("Find all usages of a type, method, or field")]
	public string FindUsages(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Member name (optional - if omitted, finds type usages)")] string? memberName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<UsageInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						if (MatchesUsage(instr, typeName, memberName, out var usageType)) {
							results.Add(new UsageInfo {
								Location = $"{type.FullName}.{method.Name}",
								UsageType = usageType,
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

	[McpServerTool, Description("Find all methods that call a specific method")]
	public string FindCallers(
		[Description("Full type name containing the method")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<CallerInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						if (IsCallInstruction(instr) && MatchesMethod(instr.Operand as IMethod, typeName, methodName)) {
							results.Add(new CallerInfo {
								TypeName = type.FullName,
								MethodName = method.Name.String,
								FullSignature = method.FullName,
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

	[McpServerTool, Description("Find all methods called by a specific method")]
	public string FindCallees(
		[Description("Full type name containing the method")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Parameter signature for overload resolution (optional)")] string? signature = null) {
		var method = FindMethod(typeName, methodName, signature);
		if (method is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' not found" });

		if (method.Body is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method has no body" });

		var results = new List<CalleeInfo>();
		foreach (var instr in method.Body.Instructions) {
			if (IsCallInstruction(instr) && instr.Operand is IMethod callee) {
				results.Add(new CalleeInfo {
					TypeName = callee.DeclaringType?.FullName ?? "Unknown",
					MethodName = callee.Name.String,
					FullSignature = callee.FullName,
					ILOffset = $"IL_{instr.Offset:X4}",
					IsVirtual = instr.OpCode.Code == Code.Callvirt
				});
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Build a call graph starting from a method")]
	public string BuildCallGraph(
		[Description("Full type name containing the root method")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Maximum depth (default: 3)")] int maxDepth = 3,
		[Description("Maximum nodes (default: 100)")] int maxNodes = 100) {
		var rootMethod = FindMethod(typeName, methodName, null);
		if (rootMethod is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' not found" });

		var graph = new CallGraph();
		var visited = new HashSet<string>();
		var queue = new Queue<(MethodDef method, int depth)>();
		queue.Enqueue((rootMethod, 0));

		while (queue.Count > 0 && graph.Nodes.Count < maxNodes) {
			var (method, depth) = queue.Dequeue();
			var nodeId = method.FullName;

			if (visited.Contains(nodeId)) continue;
			visited.Add(nodeId);

			graph.Nodes.Add(new CallGraphNode {
				Id = nodeId,
				TypeName = method.DeclaringType?.FullName ?? "Unknown",
				MethodName = method.Name.String,
				FullName = method.FullName,
				Depth = depth
			});

			if (depth >= maxDepth || method.Body is null) continue;

			foreach (var instr in method.Body.Instructions) {
				if (IsCallInstruction(instr) && instr.Operand is IMethod callee) {
					var calleeId = callee.FullName;
					graph.Edges.Add(new CallGraphEdge {
						FromId = nodeId,
						ToId = calleeId,
						ILOffset = $"IL_{instr.Offset:X4}"
					});

					if (callee is MethodDef calleeDef && !visited.Contains(calleeId)) {
						queue.Enqueue((calleeDef, depth + 1));
					}
				}
			}
		}

		graph.TotalNodes = graph.Nodes.Count;
		graph.TotalEdges = graph.Edges.Count;
		graph.Truncated = queue.Count > 0;

		return JsonSerializer.Serialize(graph, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Find all references to a type")]
	public string FindTypeReferences(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<TypeReferenceInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				// Check base type
				if (type.BaseType?.FullName == typeName) {
					results.Add(new TypeReferenceInfo {
						Location = type.FullName,
						ReferenceKind = "Inherits",
						Assembly = asmName
					});
				}

				// Check interfaces
				foreach (var iface in type.Interfaces) {
					if (iface.Interface.FullName == typeName) {
						results.Add(new TypeReferenceInfo {
							Location = type.FullName,
							ReferenceKind = "Implements",
							Assembly = asmName
						});
					}
				}

				// Check fields
				foreach (var field in type.Fields) {
					if (ContainsType(field.FieldType, typeName)) {
						results.Add(new TypeReferenceInfo {
							Location = $"{type.FullName}.{field.Name}",
							ReferenceKind = "FieldType",
							Assembly = asmName
						});
					}
				}

				if (results.Count >= maxResults)
					return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Find all references to a field")]
	public string FindFieldReferences(
		[Description("Full type name containing the field")] string typeName,
		[Description("Field name")] string fieldName,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<FieldReferenceInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						if (IsFieldInstruction(instr) && instr.Operand is IField field) {
							if (field.DeclaringType?.FullName == typeName && field.Name == fieldName) {
								results.Add(new FieldReferenceInfo {
									Location = $"{type.FullName}.{method.Name}",
									ReferenceType = GetFieldRefType(instr),
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

	[McpServerTool, Description("Find all usages of a specific string value")]
	public string FindStringUsages(
		[Description("Exact string value to search for")] string value,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<StringUsageInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var method in type.Methods) {
					if (method.Body is null) continue;

					foreach (var instr in method.Body.Instructions) {
						if (instr.OpCode.Code == Code.Ldstr && instr.Operand is string str && str == value) {
							results.Add(new StringUsageInfo {
								Value = str,
								Location = $"{type.FullName}.{method.Name}",
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

	[McpServerTool, Description("Analyze type dependencies (incoming and outgoing)")]
	public string AnalyzeDependencies(
		[Description("Full type name to analyze")] string typeName,
		[Description("Maximum results per direction (default: 50)")] int maxResults = 50) {
		var analysis = new DependencyAnalysis { TargetType = typeName };

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				// Incoming: types that depend on target
				if (type.FullName != typeName) {
					if (type.BaseType?.FullName == typeName) {
						analysis.Incoming.Add(new DependencyInfo {
							TypeName = type.FullName,
							Direction = "Incoming",
							DependencyKind = "Inherits",
							Assembly = asmName
						});
					}

					foreach (var field in type.Fields) {
						if (ContainsType(field.FieldType, typeName) && analysis.Incoming.Count < maxResults) {
							analysis.Incoming.Add(new DependencyInfo {
								TypeName = type.FullName,
								Direction = "Incoming",
								DependencyKind = "FieldType",
								Assembly = asmName
							});
							break;
						}
					}
				}

				// Outgoing: types that target depends on
				if (type.FullName == typeName) {
					if (type.BaseType is not null && type.BaseType.FullName != "System.Object") {
						analysis.Outgoing.Add(new DependencyInfo {
							TypeName = type.BaseType.FullName,
							Direction = "Outgoing",
							DependencyKind = "BaseType",
							Assembly = asmName
						});
					}

					foreach (var iface in type.Interfaces) {
						analysis.Outgoing.Add(new DependencyInfo {
							TypeName = iface.Interface.FullName,
							Direction = "Outgoing",
							DependencyKind = "Interface",
							Assembly = asmName
						});
					}

					foreach (var field in type.Fields) {
						var fieldTypeName = GetBaseTypeName(field.FieldType);
						if (fieldTypeName is not null && !fieldTypeName.StartsWith("System.") && analysis.Outgoing.Count < maxResults) {
							analysis.Outgoing.Add(new DependencyInfo {
								TypeName = fieldTypeName,
								Direction = "Outgoing",
								DependencyKind = "FieldType",
								Assembly = asmName
							});
						}
					}
				}
			}
		}

		return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
	}

	MethodDef? FindMethod(string typeName, string methodName, string? signature) {
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var type = doc.ModuleDef.Find(typeName, false) ??
				doc.ModuleDef.Types.FirstOrDefault(t => t.FullName.EndsWith(typeName, StringComparison.OrdinalIgnoreCase));
			if (type is null) continue;

			var methods = type.Methods.Where(m => m.Name == methodName).ToList();
			if (methods.Count == 0) continue;
			if (methods.Count == 1 || string.IsNullOrEmpty(signature)) return methods[0];

			var sigParts = signature.Split(',').Select(s => s.Trim()).ToArray();
			return methods.FirstOrDefault(m => MatchesSignature(m, sigParts)) ?? methods[0];
		}
		return null;
	}

	static bool MatchesSignature(MethodDef method, string[] sigParts) {
		var parameters = method.Parameters.Where(p => !p.IsHiddenThisParameter).ToList();
		if (parameters.Count != sigParts.Length) return false;
		for (int i = 0; i < sigParts.Length; i++) {
			var paramType = parameters[i].Type?.TypeName ?? "";
			if (!paramType.Contains(sigParts[i], StringComparison.OrdinalIgnoreCase)) return false;
		}
		return true;
	}

	static bool IsCallInstruction(Instruction instr) =>
		instr.OpCode.Code == Code.Call ||
		instr.OpCode.Code == Code.Callvirt ||
		instr.OpCode.Code == Code.Newobj;

	static bool IsFieldInstruction(Instruction instr) =>
		instr.OpCode.Code == Code.Ldfld ||
		instr.OpCode.Code == Code.Ldsfld ||
		instr.OpCode.Code == Code.Stfld ||
		instr.OpCode.Code == Code.Stsfld ||
		instr.OpCode.Code == Code.Ldflda ||
		instr.OpCode.Code == Code.Ldsflda;

	static string GetFieldRefType(Instruction instr) => instr.OpCode.Code switch {
		Code.Ldfld or Code.Ldsfld => "Read",
		Code.Stfld or Code.Stsfld => "Write",
		Code.Ldflda or Code.Ldsflda => "AddressOf",
		_ => "Unknown"
	};

	static bool MatchesUsage(Instruction instr, string typeName, string? memberName, out string usageType) {
		usageType = "";

		if (memberName is null) {
			// Type usage
			if (instr.Operand is ITypeDefOrRef typeRef && typeRef.FullName == typeName) {
				usageType = "TypeReference";
				return true;
			}
			if (instr.Operand is IMethod method && method.DeclaringType?.FullName == typeName) {
				usageType = "MethodCall";
				return true;
			}
			if (instr.Operand is IField field && field.DeclaringType?.FullName == typeName) {
				usageType = "FieldAccess";
				return true;
			}
		}
		else {
			// Member usage
			if (instr.Operand is IMethod method && method.DeclaringType?.FullName == typeName && method.Name == memberName) {
				usageType = IsCallInstruction(instr) ? "Call" : "Reference";
				return true;
			}
			if (instr.Operand is IField field && field.DeclaringType?.FullName == typeName && field.Name == memberName) {
				usageType = GetFieldRefType(instr);
				return true;
			}
		}

		return false;
	}

	static bool MatchesMethod(IMethod? method, string typeName, string methodName) {
		if (method is null) return false;
		return method.DeclaringType?.FullName == typeName && method.Name == methodName;
	}

	static bool ContainsType(TypeSig? sig, string typeName) {
		if (sig is null) return false;
		if (sig.FullName == typeName) return true;
		if (sig is GenericInstSig gis) {
			if (gis.GenericType?.FullName == typeName) return true;
			return gis.GenericArguments.Any(a => ContainsType(a, typeName));
		}
		return false;
	}

	static string? GetBaseTypeName(TypeSig? sig) {
		if (sig is null) return null;
		if (sig is GenericInstSig gis) return gis.GenericType?.FullName;
		return sig.FullName;
	}
}

