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

namespace dnSpy.Mcp.Models;

/// <summary>
/// Summary of a loaded assembly/document in dnSpy.
/// </summary>
public sealed class AssemblyListItem {
	/// <summary>Full file path to the assembly.</summary>
	public string Filename { get; set; } = "";
	/// <summary>Short display name (typically filename without path).</summary>
	public string ShortName { get; set; } = "";
	/// <summary>Document kind: "dotnet" for managed assemblies, "native" for PE files, "other" otherwise.</summary>
	public string Kind { get; set; } = "";
	/// <summary>True if this is a managed .NET assembly.</summary>
	public bool IsManaged { get; set; }
	/// <summary>Full assembly name if managed, null otherwise.</summary>
	public string? AssemblyName { get; set; }
	/// <summary>Module name if managed, null otherwise.</summary>
	public string? ModuleName { get; set; }
	/// <summary>Number of modules in the assembly.</summary>
	public int ModuleCount { get; set; }
}

/// <summary>
/// Summary of a type for list/browse operations.
/// </summary>
public sealed class TypeListItem {
	/// <summary>Full type name including namespace (e.g., "System.Collections.Generic.List`1").</summary>
	public string FullName { get; set; } = "";
	/// <summary>Namespace containing the type.</summary>
	public string Namespace { get; set; } = "";
	/// <summary>Simple type name without namespace.</summary>
	public string Name { get; set; } = "";
	/// <summary>True if the type has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if this is a class (not interface, struct, or enum).</summary>
	public bool IsClass { get; set; }
	/// <summary>True if this is an interface.</summary>
	public bool IsInterface { get; set; }
	/// <summary>True if this is an enum.</summary>
	public bool IsEnum { get; set; }
	/// <summary>True if this is a value type (struct or enum).</summary>
	public bool IsValueType { get; set; }
	/// <summary>Full name of the base type, or null if none.</summary>
	public string? BaseType { get; set; }
	/// <summary>Number of methods defined in this type.</summary>
	public int MethodCount { get; set; }
	/// <summary>Number of fields defined in this type.</summary>
	public int FieldCount { get; set; }
	/// <summary>Number of properties defined in this type.</summary>
	public int PropertyCount { get; set; }
}

/// <summary>
/// Result from a type search operation.
/// </summary>
public sealed class TypeSearchResult {
	/// <summary>Full type name including namespace.</summary>
	public string FullName { get; set; } = "";
	/// <summary>Assembly containing this type.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>True if the type has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>Type kind: "class", "interface", "struct", "enum", or "delegate".</summary>
	public string Kind { get; set; } = "";
}

/// <summary>
/// Detailed information about a type including its members.
/// </summary>
public sealed class TypeInfo {
	/// <summary>Full type name including namespace.</summary>
	public string FullName { get; set; } = "";
	/// <summary>Namespace containing the type.</summary>
	public string Namespace { get; set; } = "";
	/// <summary>Simple type name without namespace.</summary>
	public string Name { get; set; } = "";
	/// <summary>Assembly containing this type.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>Type kind: "class", "interface", "struct", "enum", or "delegate".</summary>
	public string Kind { get; set; } = "";
	/// <summary>Full name of the base type, or null if none.</summary>
	public string? BaseType { get; set; }
	/// <summary>List of interface full names implemented by this type.</summary>
	public List<string> Interfaces { get; set; } = new();
	/// <summary>True if the type is abstract (cannot be instantiated directly).</summary>
	public bool IsAbstract { get; set; }
	/// <summary>True if the type is sealed (cannot be inherited).</summary>
	public bool IsSealed { get; set; }
	/// <summary>True if the type has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>Generic type parameter names (e.g., ["T", "TResult"] for Func&lt;T, TResult&gt;).</summary>
	public List<string> GenericParameters { get; set; } = new();
	/// <summary>Methods defined in this type.</summary>
	public List<TypeMethodSummary> Methods { get; set; } = new();
	/// <summary>Fields defined in this type.</summary>
	public List<TypeFieldSummary> Fields { get; set; } = new();
	/// <summary>Properties defined in this type.</summary>
	public List<TypePropertySummary> Properties { get; set; } = new();
}

/// <summary>
/// Brief summary of a method within a type.
/// </summary>
public sealed class TypeMethodSummary {
	/// <summary>Method name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Return type name, or null for void/constructors.</summary>
	public string? ReturnType { get; set; }
	/// <summary>List of parameter type names.</summary>
	public List<string> Parameters { get; set; } = new();
	/// <summary>True if the method has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if this is a constructor (.ctor or .cctor).</summary>
	public bool IsConstructor { get; set; }
}

/// <summary>
/// Brief summary of a field within a type.
/// </summary>
public sealed class TypeFieldSummary {
	/// <summary>Field name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Field type name.</summary>
	public string? Type { get; set; }
	/// <summary>True if the field has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the field is static.</summary>
	public bool IsStatic { get; set; }
}

/// <summary>
/// Brief summary of a property within a type.
/// </summary>
public sealed class TypePropertySummary {
	/// <summary>Property name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Property type name.</summary>
	public string? Type { get; set; }
	/// <summary>True if the property has a getter.</summary>
	public bool HasGetter { get; set; }
	/// <summary>True if the property has a setter.</summary>
	public bool HasSetter { get; set; }
}

