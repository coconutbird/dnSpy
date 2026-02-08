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
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for type hierarchy analysis
/// </summary>
[McpServerToolType]
public sealed class TypeHierarchyTools {
	readonly DnSpyServices services;

	public TypeHierarchyTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("Get the full type hierarchy (base types and interfaces) for a type")]
	public string GetTypeHierarchy(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var hierarchy = new TypeHierarchy {
			TypeName = type.FullName
		};

		// Build base type chain
		var current = type.BaseType;
		while (current is not null) {
			hierarchy.BaseTypes.Add(current.FullName);
			current = ResolveBaseType(current);
		}

		// Direct interfaces
		foreach (var iface in type.Interfaces) {
			hierarchy.Interfaces.Add(iface.Interface.FullName);
		}

		// All interfaces (including inherited)
		var allInterfaces = new HashSet<string>();
		CollectAllInterfaces(type, allInterfaces);
		hierarchy.AllInterfaces = allInterfaces.ToList();

		return JsonSerializer.Serialize(hierarchy, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Find all types that derive from a specific type")]
	public string FindDerivedTypes(
		[Description("Full type name (e.g., MyNamespace.MyBaseClass)")] string typeName,
		[Description("Include indirect descendants (default: true)")] bool includeIndirect = true,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<DerivedTypeInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				if (type.FullName == typeName) continue;

				// Check direct inheritance
				if (type.BaseType?.FullName == typeName) {
					results.Add(new DerivedTypeInfo {
						TypeName = type.FullName,
						Assembly = asmName,
						IsDirect = true,
						Kind = type.IsInterface ? "Interface" : type.IsValueType ? "Struct" : "Class"
					});
				}
				else if (includeIndirect && InheritsFrom(type, typeName)) {
					results.Add(new DerivedTypeInfo {
						TypeName = type.FullName,
						Assembly = asmName,
						IsDirect = false,
						Kind = type.IsInterface ? "Interface" : type.IsValueType ? "Struct" : "Class"
					});
				}

				if (results.Count >= maxResults)
					return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Find all implementations of an interface or overrides of a virtual method")]
	public string FindImplementations(
		[Description("Full type name (interface or class with virtual members)")] string typeName,
		[Description("Member name (optional - if omitted, finds all type implementations)")] string? memberName = null,
		[Description("Maximum results (default: 100)")] int maxResults = 100) {
		var results = new List<ImplementationInfo>();

		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var asmName = doc.AssemblyDef?.Name?.String ?? doc.Filename;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				if (type.FullName == typeName) continue;

				// Check if type implements the interface
				if (ImplementsInterface(type, typeName)) {
					if (string.IsNullOrEmpty(memberName)) {
						results.Add(new ImplementationInfo {
							TypeName = type.FullName,
							MemberName = "",
							MemberKind = "Type",
							Assembly = asmName,
							IsExplicit = false
						});
					}
					else {
						// Find specific member implementations
						foreach (var method in type.Methods) {
							if (IsImplementationOf(method, typeName, memberName)) {
								results.Add(new ImplementationInfo {
									TypeName = type.FullName,
									MemberName = method.Name.String,
									MemberKind = "Method",
									Assembly = asmName,
									IsExplicit = method.Name.String.Contains('.')
								});
							}
						}
					}
				}

				if (results.Count >= maxResults)
					return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	TypeDef? FindType(string typeName) {
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var type = doc.ModuleDef.Find(typeName, false) ??
				doc.ModuleDef.Types.FirstOrDefault(t => t.FullName.EndsWith(typeName, StringComparison.OrdinalIgnoreCase));
			if (type is not null) return type;
		}
		return null;
	}

	static ITypeDefOrRef? ResolveBaseType(ITypeDefOrRef? typeRef) {
		if (typeRef is null) return null;
		if (typeRef is TypeDef td) return td.BaseType;
		if (typeRef is TypeRef tr) {
			var resolved = tr.Resolve();
			return resolved?.BaseType;
		}
		return null;
	}

	static void CollectAllInterfaces(TypeDef type, HashSet<string> interfaces) {
		foreach (var iface in type.Interfaces) {
			var ifaceName = iface.Interface.FullName;
			if (interfaces.Add(ifaceName)) {
				// Recursively get interfaces from the interface itself
				if (iface.Interface is TypeDef ifaceDef) {
					CollectAllInterfaces(ifaceDef, interfaces);
				}
				else if (iface.Interface is TypeRef tr) {
					var resolved = tr.Resolve();
					if (resolved is not null)
						CollectAllInterfaces(resolved, interfaces);
				}
			}
		}

		// Also get interfaces from base type
		if (type.BaseType is not null) {
			if (type.BaseType is TypeDef baseDef) {
				CollectAllInterfaces(baseDef, interfaces);
			}
			else if (type.BaseType is TypeRef tr) {
				var resolved = tr.Resolve();
				if (resolved is not null)
					CollectAllInterfaces(resolved, interfaces);
			}
		}
	}

	static bool InheritsFrom(TypeDef type, string baseTypeName) {
		var current = type.BaseType;
		while (current is not null) {
			if (current.FullName == baseTypeName) return true;
			current = ResolveBaseType(current);
		}
		return false;
	}

	static bool ImplementsInterface(TypeDef type, string interfaceName) {
		var allInterfaces = new HashSet<string>();
		CollectAllInterfaces(type, allInterfaces);
		return allInterfaces.Contains(interfaceName);
	}

	static bool IsImplementationOf(MethodDef method, string interfaceTypeName, string memberName) {
		// Check explicit implementation
		if (method.Name.String == $"{interfaceTypeName}.{memberName}") return true;

		// Check if method name matches and type implements the interface
		if (method.Name.String == memberName && method.DeclaringType is not null) {
			return ImplementsInterface(method.DeclaringType, interfaceTypeName);
		}

		// Check overrides
		foreach (var ovr in method.Overrides) {
			if (ovr.MethodDeclaration?.DeclaringType?.FullName == interfaceTypeName &&
				ovr.MethodDeclaration?.Name == memberName) {
				return true;
			}
		}

		return false;
	}
}
