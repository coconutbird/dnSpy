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

public sealed class FieldSearchResult {
	public string TypeName { get; set; } = "";
	public string FieldName { get; set; } = "";
	public string? FieldType { get; set; }
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
	public bool IsLiteral { get; set; }
	public bool IsInitOnly { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class FieldListItem {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
	public string DeclaringType { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
	public bool IsLiteral { get; set; }
	public bool IsInitOnly { get; set; }
}

public sealed class FieldInfo {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
	public string DeclaringType { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool IsPrivate { get; set; }
	public bool IsFamily { get; set; }
	public bool IsAssembly { get; set; }
	public bool IsStatic { get; set; }
	public bool IsLiteral { get; set; }
	public bool IsInitOnly { get; set; }
	public bool HasConstant { get; set; }
	public string? ConstantValue { get; set; }
	public string MetadataToken { get; set; } = "";
	public uint? FieldOffset { get; set; }
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Properties

public sealed class PropertySearchResult {
	public string TypeName { get; set; } = "";
	public string PropertyName { get; set; } = "";
	public string? PropertyType { get; set; }
	public bool HasGetter { get; set; }
	public bool HasSetter { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class PropertyListItem {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
	public string DeclaringType { get; set; } = "";
	public bool CanRead { get; set; }
	public bool CanWrite { get; set; }
	public bool IsStatic { get; set; }
}

public sealed class PropertyInfo {
	public string Name { get; set; } = "";
	public string? PropertyType { get; set; }
	public string DeclaringType { get; set; } = "";
	public bool CanRead { get; set; }
	public bool CanWrite { get; set; }
	public string? GetterName { get; set; }
	public string? SetterName { get; set; }
	public bool IsStatic { get; set; }
	public bool IsIndexer { get; set; }
	public List<string>? IndexerParameters { get; set; }
	public string MetadataToken { get; set; } = "";
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Events

public sealed class EventSearchResult {
	public string TypeName { get; set; } = "";
	public string EventName { get; set; } = "";
	public string? EventHandlerType { get; set; }
	public bool HasAdd { get; set; }
	public bool HasRemove { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class EventListItem {
	public string Name { get; set; } = "";
	public string? EventHandlerType { get; set; }
	public bool HasAdd { get; set; }
	public bool HasRemove { get; set; }
	public bool HasRaise { get; set; }
	public bool IsStatic { get; set; }
}

public sealed class EventInfo {
	public string Name { get; set; } = "";
	public string? EventHandlerType { get; set; }
	public string DeclaringType { get; set; } = "";
	public string? AddMethodName { get; set; }
	public string? RemoveMethodName { get; set; }
	public string? RaiseMethodName { get; set; }
	public bool IsStatic { get; set; }
	public string MetadataToken { get; set; } = "";
	public List<string?> CustomAttributes { get; set; } = new();
}

#endregion

#region Methods

public sealed class MethodSearchResult {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string? ReturnType { get; set; }
	public List<string> Parameters { get; set; } = new();
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class MethodListItem {
	public string Name { get; set; } = "";
	public string? ReturnType { get; set; }
	public List<string> Parameters { get; set; } = new();
	public string DeclaringType { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
	public bool IsVirtual { get; set; }
	public bool IsConstructor { get; set; }
}

public sealed class MethodInfo {
	public string Name { get; set; } = "";
	public string FullName { get; set; } = "";
	public string? ReturnType { get; set; }
	public List<MethodParameter> Parameters { get; set; } = new();
	public bool IsPublic { get; set; }
	public bool IsPrivate { get; set; }
	public bool IsStatic { get; set; }
	public bool IsVirtual { get; set; }
	public bool IsAbstract { get; set; }
	public bool IsFinal { get; set; }
	public bool IsConstructor { get; set; }
	public bool IsGeneric { get; set; }
	public List<string> GenericParameters { get; set; } = new();
	public string MetadataToken { get; set; } = "";
	public string RVA { get; set; } = "";
	public List<string?> CustomAttributes { get; set; } = new();
}

public sealed class MethodParameter {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
}

#endregion

