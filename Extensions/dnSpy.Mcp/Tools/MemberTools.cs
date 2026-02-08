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
/// MCP tools for working with type members (fields, properties, events)
/// </summary>
[McpServerToolType]
public sealed class MemberTools {
	readonly DnSpyServices services;

	public MemberTools(DnSpyServices services) {
		this.services = services;
	}

	#region Fields

	[McpServerTool, Description("Search fields by name pattern across all loaded assemblies")]
	public string FindFields(
		[Description("Field name pattern to search for")] string pattern,
		[Description("Assembly name filter (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 50)")] int maxResults = 50) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<FieldSearchResult>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (assemblyName is not null && !MatchesAssemblyName(doc, assemblyName))
				continue;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var field in type.Fields) {
					if (field.Name.String.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
						results.Add(new FieldSearchResult {
							TypeName = type.FullName,
							FieldName = field.Name.String,
							FieldType = field.FieldType?.FullName,
							IsPublic = field.IsPublic,
							IsStatic = field.IsStatic,
							IsLiteral = field.IsLiteral,
							IsInitOnly = field.IsInitOnly,
							Assembly = doc.GetShortName()
						});

						if (results.Count >= maxResults)
							return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
					}
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("List all fields in a type")]
	public string ListFields(
		[Description("Full type name")] string typeName,
		[Description("Include inherited fields (default: false)")] bool includeInherited = false,
		[Description("Include private fields (default: true)")] bool includePrivate = true) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var fields = new List<FieldListItem>();
		CollectFields(type, fields, includeInherited, includePrivate, new HashSet<string>());

		return JsonSerializer.Serialize(fields, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific field")]
	public string GetFieldInfo(
		[Description("Full type name")] string typeName,
		[Description("Field name")] string fieldName) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var field = type.Fields.FirstOrDefault(f =>
			f.Name.String.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		if (field is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Field '{fieldName}' not found in type '{typeName}'" });

		var info = new FieldInfo {
			Name = field.Name.String,
			Type = field.FieldType?.FullName,
			DeclaringType = type.FullName,
			IsPublic = field.IsPublic,
			IsPrivate = field.IsPrivate,
			IsFamily = field.IsFamily,
			IsAssembly = field.IsAssembly,
			IsStatic = field.IsStatic,
			IsLiteral = field.IsLiteral,
			IsInitOnly = field.IsInitOnly,
			HasConstant = field.HasConstant,
			ConstantValue = field.HasConstant ? field.Constant?.Value?.ToString() : null,
			MetadataToken = field.MDToken.Raw.ToString("X8"),
			FieldOffset = field.FieldOffset,
			CustomAttributes = field.CustomAttributes.Select(a => a.AttributeType?.FullName).ToList()
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	#endregion

	#region Properties

	[McpServerTool, Description("Search properties by name pattern across all loaded assemblies")]
	public string FindProperties(
		[Description("Property name pattern to search for")] string pattern,
		[Description("Assembly name filter (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 50)")] int maxResults = 50) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<PropertySearchResult>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (assemblyName is not null && !MatchesAssemblyName(doc, assemblyName))
				continue;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var prop in type.Properties) {
					if (prop.Name.String.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
						results.Add(new PropertySearchResult {
							TypeName = type.FullName,
							PropertyName = prop.Name.String,
							PropertyType = prop.PropertySig?.RetType?.FullName,
							HasGetter = prop.GetMethod is not null,
							HasSetter = prop.SetMethod is not null,
							Assembly = doc.GetShortName()
						});

						if (results.Count >= maxResults)
							return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
					}
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("List all properties in a type")]
	public string ListProperties(
		[Description("Full type name")] string typeName,
		[Description("Include inherited properties (default: false)")] bool includeInherited = false) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var properties = new List<PropertyListItem>();
		CollectProperties(type, properties, includeInherited, new HashSet<string>());

		return JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific property")]
	public string GetPropertyInfo(
		[Description("Full type name")] string typeName,
		[Description("Property name")] string propertyName) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var prop = type.Properties.FirstOrDefault(p =>
			p.Name.String.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
		if (prop is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Property '{propertyName}' not found in type '{typeName}'" });

		var info = new PropertyInfo {
			Name = prop.Name.String,
			PropertyType = prop.PropertySig?.RetType?.FullName,
			DeclaringType = type.FullName,
			CanRead = prop.GetMethod is not null,
			CanWrite = prop.SetMethod is not null,
			GetterName = prop.GetMethod?.Name.String,
			SetterName = prop.SetMethod?.Name.String,
			IsStatic = prop.GetMethod?.IsStatic ?? prop.SetMethod?.IsStatic ?? false,
			IsIndexer = prop.PropertySig?.Params.Count > 0,
			IndexerParameters = prop.PropertySig?.Params.Select(p => p.FullName).ToList(),
			MetadataToken = prop.MDToken.Raw.ToString("X8"),
			CustomAttributes = prop.CustomAttributes.Select(a => a.AttributeType?.FullName).ToList()
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	#endregion

	#region Events

	[McpServerTool, Description("Search events by name pattern across all loaded assemblies")]
	public string FindEvents(
		[Description("Event name pattern to search for")] string pattern,
		[Description("Assembly name filter (optional)")] string? assemblyName = null,
		[Description("Maximum results (default: 50)")] int maxResults = 50) {
		var docs = services.DocumentService.GetDocuments();
		var results = new List<EventSearchResult>();

		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;

			if (assemblyName is not null && !MatchesAssemblyName(doc, assemblyName))
				continue;

			foreach (var type in doc.ModuleDef.GetTypes()) {
				foreach (var evt in type.Events) {
					if (evt.Name.String.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
						results.Add(new EventSearchResult {
							TypeName = type.FullName,
							EventName = evt.Name.String,
							EventHandlerType = evt.EventType?.FullName,
							HasAdd = evt.AddMethod is not null,
							HasRemove = evt.RemoveMethod is not null,
							Assembly = doc.GetShortName()
						});

						if (results.Count >= maxResults)
							return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
					}
				}
			}
		}

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("List all events in a type")]
	public string ListEvents(
		[Description("Full type name")] string typeName) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var events = type.Events.Select(e => new EventListItem {
			Name = e.Name.String,
			EventHandlerType = e.EventType?.FullName,
			HasAdd = e.AddMethod is not null,
			HasRemove = e.RemoveMethod is not null,
			HasRaise = e.InvokeMethod is not null,
			IsStatic = e.AddMethod?.IsStatic ?? false
		}).ToList();

		return JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific event")]
	public string GetEventInfo(
		[Description("Full type name")] string typeName,
		[Description("Event name")] string eventName) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var evt = type.Events.FirstOrDefault(e =>
			e.Name.String.Equals(eventName, StringComparison.OrdinalIgnoreCase));
		if (evt is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Event '{eventName}' not found in type '{typeName}'" });

		var info = new EventInfo {
			Name = evt.Name.String,
			EventHandlerType = evt.EventType?.FullName,
			DeclaringType = type.FullName,
			AddMethodName = evt.AddMethod?.Name.String,
			RemoveMethodName = evt.RemoveMethod?.Name.String,
			RaiseMethodName = evt.InvokeMethod?.Name.String,
			IsStatic = evt.AddMethod?.IsStatic ?? false,
			MetadataToken = evt.MDToken.Raw.ToString("X8"),
			CustomAttributes = evt.CustomAttributes.Select(a => a.AttributeType?.FullName).ToList()
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	#endregion

	#region Methods (additional)

	[McpServerTool, Description("List all methods in a type with detailed information")]
	public string ListMethods(
		[Description("Full type name")] string typeName,
		[Description("Include inherited methods (default: false)")] bool includeInherited = false,
		[Description("Include private methods (default: true)")] bool includePrivate = true) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var methods = new List<MethodListItem>();
		CollectMethods(type, methods, includeInherited, includePrivate, new HashSet<string>());

		return JsonSerializer.Serialize(methods, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get detailed information about a specific method")]
	public string GetMethodInfo(
		[Description("Full type name")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Parameter signature for overload resolution (optional, e.g., 'int, string')")] string? signature = null) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var methods = type.Methods.Where(m =>
			m.Name.String.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

		if (methods.Count == 0)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{methodName}' not found in type '{typeName}'" });

		// If signature provided, filter by it
		if (signature is not null) {
			var sigParts = signature.Split(',').Select(s => s.Trim()).ToArray();
			methods = methods.Where(m => {
				var parms = m.Parameters.Where(p => !p.IsHiddenThisParameter).ToList();
				if (parms.Count != sigParts.Length) return false;
				for (int i = 0; i < sigParts.Length; i++) {
					if (!parms[i].Type.FullName.Contains(sigParts[i], StringComparison.OrdinalIgnoreCase))
						return false;
				}
				return true;
			}).ToList();
		}

		var results = methods.Select(m => new MethodInfo {
			Name = m.Name.String,
			FullName = m.FullName,
			ReturnType = m.ReturnType?.FullName,
			Parameters = m.Parameters.Where(p => !p.IsHiddenThisParameter)
				.Select(p => new MethodParameter { Name = p.Name, Type = p.Type?.FullName }).ToList(),
			IsPublic = m.IsPublic,
			IsPrivate = m.IsPrivate,
			IsStatic = m.IsStatic,
			IsVirtual = m.IsVirtual,
			IsAbstract = m.IsAbstract,
			IsFinal = m.IsFinal,
			IsConstructor = m.IsConstructor,
			IsGeneric = m.HasGenericParameters,
			GenericParameters = m.GenericParameters.Select(g => g.Name.String).ToList(),
			MetadataToken = m.MDToken.Raw.ToString("X8"),
			RVA = m.RVA.ToString("X8"),
			CustomAttributes = m.CustomAttributes.Select(a => a.AttributeType?.FullName).ToList()
		}).ToList();

		if (results.Count == 1)
			return JsonSerializer.Serialize(results[0], new JsonSerializerOptions { WriteIndented = true });

		return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
	}

	#endregion

	#region Helpers

	TypeDef? FindType(string typeName) {
		var docs = services.DocumentService.GetDocuments();
		foreach (var doc in docs) {
			if (doc.ModuleDef is null)
				continue;
			var type = doc.ModuleDef.Find(typeName, true);
			if (type is not null)
				return type;
		}
		return null;
	}

	bool MatchesAssemblyName(IDsDocument doc, string name) {
		return doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
			(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false);
	}

	void CollectFields(TypeDef type, List<FieldListItem> fields, bool includeInherited, bool includePrivate, HashSet<string> seen) {
		foreach (var field in type.Fields) {
			if (!includePrivate && field.IsPrivate)
				continue;
			if (seen.Contains(field.Name.String))
				continue;
			seen.Add(field.Name.String);

			fields.Add(new FieldListItem {
				Name = field.Name.String,
				Type = field.FieldType?.FullName,
				DeclaringType = type.FullName,
				IsPublic = field.IsPublic,
				IsStatic = field.IsStatic,
				IsLiteral = field.IsLiteral,
				IsInitOnly = field.IsInitOnly
			});
		}

		if (includeInherited && type.BaseType is not null) {
			var baseType = type.BaseType.ResolveTypeDef();
			if (baseType is not null && baseType.FullName != "System.Object")
				CollectFields(baseType, fields, true, includePrivate, seen);
		}
	}

	void CollectProperties(TypeDef type, List<PropertyListItem> properties, bool includeInherited, HashSet<string> seen) {
		foreach (var prop in type.Properties) {
			if (seen.Contains(prop.Name.String))
				continue;
			seen.Add(prop.Name.String);

			properties.Add(new PropertyListItem {
				Name = prop.Name.String,
				Type = prop.PropertySig?.RetType?.FullName,
				DeclaringType = type.FullName,
				CanRead = prop.GetMethod is not null,
				CanWrite = prop.SetMethod is not null,
				IsStatic = prop.GetMethod?.IsStatic ?? prop.SetMethod?.IsStatic ?? false
			});
		}

		if (includeInherited && type.BaseType is not null) {
			var baseType = type.BaseType.ResolveTypeDef();
			if (baseType is not null && baseType.FullName != "System.Object")
				CollectProperties(baseType, properties, true, seen);
		}
	}

	void CollectMethods(TypeDef type, List<MethodListItem> methods, bool includeInherited, bool includePrivate, HashSet<string> seen) {
		foreach (var method in type.Methods) {
			if (!includePrivate && method.IsPrivate)
				continue;

			var sig = $"{method.Name}({string.Join(",", method.Parameters.Where(p => !p.IsHiddenThisParameter).Select(p => p.Type?.FullName))})";
			if (seen.Contains(sig))
				continue;
			seen.Add(sig);

			methods.Add(new MethodListItem {
				Name = method.Name.String,
				ReturnType = method.ReturnType?.FullName,
				Parameters = method.Parameters.Where(p => !p.IsHiddenThisParameter)
					.Select(p => $"{p.Type?.FullName} {p.Name}").ToList(),
				DeclaringType = type.FullName,
				IsPublic = method.IsPublic,
				IsStatic = method.IsStatic,
				IsVirtual = method.IsVirtual,
				IsConstructor = method.IsConstructor
			});
		}

		if (includeInherited && type.BaseType is not null) {
			var baseType = type.BaseType.ResolveTypeDef();
			if (baseType is not null && baseType.FullName != "System.Object")
				CollectMethods(baseType, methods, true, includePrivate, seen);
		}
	}

	#endregion
}
