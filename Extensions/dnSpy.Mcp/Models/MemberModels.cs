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

#region Fields

/// <summary>
/// Result from a field search operation.
/// </summary>
public sealed class FieldSearchResult {
	/// <summary>Full name of the type containing this field.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the field.</summary>
	public string FieldName { get; set; } = "";
	/// <summary>Type of the field.</summary>
	public string? FieldType { get; set; }
	/// <summary>True if the field has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the field is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if the field is a compile-time constant (const).</summary>
	public bool IsLiteral { get; set; }
	/// <summary>True if the field is readonly (can only be set in constructor).</summary>
	public bool IsInitOnly { get; set; }
	/// <summary>Assembly containing this field.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Summary of a field for list operations.
/// </summary>
public sealed class FieldListItem {
	/// <summary>Name of the field.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the field.</summary>
	public string? Type { get; set; }
	/// <summary>Full name of the type containing this field.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>True if the field has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the field is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if the field is a compile-time constant (const).</summary>
	public bool IsLiteral { get; set; }
	/// <summary>True if the field is readonly.</summary>
	public bool IsInitOnly { get; set; }
}

/// <summary>
/// Detailed information about a field.
/// </summary>
public sealed class FieldInfo {
	/// <summary>Name of the field.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the field.</summary>
	public string? Type { get; set; }
	/// <summary>Full name of the type containing this field.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>True if the field has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the field has private visibility.</summary>
	public bool IsPrivate { get; set; }
	/// <summary>True if the field has protected (family) visibility.</summary>
	public bool IsFamily { get; set; }
	/// <summary>True if the field has internal (assembly) visibility.</summary>
	public bool IsAssembly { get; set; }
	/// <summary>True if the field is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if the field is a compile-time constant (const).</summary>
	public bool IsLiteral { get; set; }
	/// <summary>True if the field is readonly.</summary>
	public bool IsInitOnly { get; set; }
	/// <summary>True if the field has a constant value.</summary>
	public bool HasConstant { get; set; }
	/// <summary>String representation of the constant value, if any.</summary>
	public string? ConstantValue { get; set; }
	/// <summary>Metadata token in hex format (e.g., "0x04000001").</summary>
	public string MetadataToken { get; set; } = "";
	/// <summary>Explicit field offset for explicit layout types, null otherwise.</summary>
	public uint? FieldOffset { get; set; }
	/// <summary>Custom attributes applied to this field.</summary>
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Properties

/// <summary>
/// Result from a property search operation.
/// </summary>
public sealed class PropertySearchResult {
	/// <summary>Full name of the type containing this property.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the property.</summary>
	public string PropertyName { get; set; } = "";
	/// <summary>Type of the property.</summary>
	public string? PropertyType { get; set; }
	/// <summary>True if the property has a getter.</summary>
	public bool HasGetter { get; set; }
	/// <summary>True if the property has a setter.</summary>
	public bool HasSetter { get; set; }
	/// <summary>Assembly containing this property.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Summary of a property for list operations.
/// </summary>
public sealed class PropertyListItem {
	/// <summary>Name of the property.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the property.</summary>
	public string? Type { get; set; }
	/// <summary>Full name of the type containing this property.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>True if the property can be read (has getter).</summary>
	public bool CanRead { get; set; }
	/// <summary>True if the property can be written (has setter).</summary>
	public bool CanWrite { get; set; }
	/// <summary>True if the property is static.</summary>
	public bool IsStatic { get; set; }
}

/// <summary>
/// Detailed information about a property.
/// </summary>
public sealed class PropertyInfo {
	/// <summary>Name of the property.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the property.</summary>
	public string? PropertyType { get; set; }
	/// <summary>Full name of the type containing this property.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>True if the property can be read (has getter).</summary>
	public bool CanRead { get; set; }
	/// <summary>True if the property can be written (has setter).</summary>
	public bool CanWrite { get; set; }
	/// <summary>Name of the getter method, if any.</summary>
	public string? GetterName { get; set; }
	/// <summary>Name of the setter method, if any.</summary>
	public string? SetterName { get; set; }
	/// <summary>True if the property is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if this is an indexer property (has parameters).</summary>
	public bool IsIndexer { get; set; }
	/// <summary>Indexer parameter types, if this is an indexer.</summary>
	public List<string>? IndexerParameters { get; set; }
	/// <summary>Metadata token in hex format (e.g., "0x17000001").</summary>
	public string MetadataToken { get; set; } = "";
	/// <summary>Custom attributes applied to this property.</summary>
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Events

/// <summary>
/// Result from an event search operation.
/// </summary>
public sealed class EventSearchResult {
	/// <summary>Full name of the type containing this event.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the event.</summary>
	public string EventName { get; set; } = "";
	/// <summary>Type of the event handler delegate.</summary>
	public string? EventHandlerType { get; set; }
	/// <summary>True if the event has an add accessor.</summary>
	public bool HasAdd { get; set; }
	/// <summary>True if the event has a remove accessor.</summary>
	public bool HasRemove { get; set; }
	/// <summary>Assembly containing this event.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Summary of an event for list operations.
/// </summary>
public sealed class EventListItem {
	/// <summary>Name of the event.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the event handler delegate.</summary>
	public string? EventHandlerType { get; set; }
	/// <summary>True if the event has an add accessor.</summary>
	public bool HasAdd { get; set; }
	/// <summary>True if the event has a remove accessor.</summary>
	public bool HasRemove { get; set; }
	/// <summary>True if the event has a raise accessor (rare).</summary>
	public bool HasRaise { get; set; }
	/// <summary>True if the event is static.</summary>
	public bool IsStatic { get; set; }
}

/// <summary>
/// Detailed information about an event.
/// </summary>
public sealed class EventInfo {
	/// <summary>Name of the event.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the event handler delegate.</summary>
	public string? EventHandlerType { get; set; }
	/// <summary>Full name of the type containing this event.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>Name of the add accessor method.</summary>
	public string? AddMethodName { get; set; }
	/// <summary>Name of the remove accessor method.</summary>
	public string? RemoveMethodName { get; set; }
	/// <summary>Name of the raise accessor method (rare).</summary>
	public string? RaiseMethodName { get; set; }
	/// <summary>True if the event is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>Metadata token in hex format (e.g., "0x14000001").</summary>
	public string MetadataToken { get; set; } = "";
	/// <summary>Custom attributes applied to this event.</summary>
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Methods

/// <summary>
/// Result from a method search operation.
/// </summary>
public sealed class MethodSearchResult {
	/// <summary>Full name of the type containing this method.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Return type of the method.</summary>
	public string? ReturnType { get; set; }
	/// <summary>List of parameter type names.</summary>
	public List<string> Parameters { get; set; } = new();
	/// <summary>True if the method has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>Assembly containing this method.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Summary of a method for list operations.
/// </summary>
public sealed class MethodListItem {
	/// <summary>Name of the method.</summary>
	public string Name { get; set; } = "";
	/// <summary>Return type of the method.</summary>
	public string? ReturnType { get; set; }
	/// <summary>List of parameter type names.</summary>
	public List<string> Parameters { get; set; } = new();
	/// <summary>Full name of the type containing this method.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>True if the method has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if the method is virtual.</summary>
	public bool IsVirtual { get; set; }
	/// <summary>True if this is a constructor (.ctor or .cctor).</summary>
	public bool IsConstructor { get; set; }
}

/// <summary>
/// Detailed information about a method.
/// </summary>
public sealed class MethodInfo {
	/// <summary>Name of the method.</summary>
	public string Name { get; set; } = "";
	/// <summary>Full method signature including type and parameters.</summary>
	public string FullName { get; set; } = "";
	/// <summary>Return type of the method.</summary>
	public string? ReturnType { get; set; }
	/// <summary>Method parameters with names and types.</summary>
	public List<MethodParameter> Parameters { get; set; } = new();
	/// <summary>True if the method has public visibility.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the method has private visibility.</summary>
	public bool IsPrivate { get; set; }
	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; set; }
	/// <summary>True if the method is virtual.</summary>
	public bool IsVirtual { get; set; }
	/// <summary>True if the method is abstract (no implementation).</summary>
	public bool IsAbstract { get; set; }
	/// <summary>True if the method is sealed (cannot be overridden).</summary>
	public bool IsFinal { get; set; }
	/// <summary>True if this is a constructor (.ctor or .cctor).</summary>
	public bool IsConstructor { get; set; }
	/// <summary>True if the method has generic type parameters.</summary>
	public bool IsGeneric { get; set; }
	/// <summary>Generic type parameter names.</summary>
	public List<string> GenericParameters { get; set; } = new();
	/// <summary>Metadata token in hex format (e.g., "0x06000001").</summary>
	public string MetadataToken { get; set; } = "";
	/// <summary>Relative Virtual Address of the method body in hex format.</summary>
	public string RVA { get; set; } = "";
	/// <summary>Custom attributes applied to this method.</summary>
	public List<string?> CustomAttributes { get; set; } = new();
}

/// <summary>
/// A method parameter with name and type.
/// </summary>
public sealed class MethodParameter {
	/// <summary>Parameter name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Parameter type.</summary>
	public string? Type { get; set; }
}

#endregion

