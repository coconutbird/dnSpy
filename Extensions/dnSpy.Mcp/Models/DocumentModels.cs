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

public sealed class AssemblyListItem {
	public string Filename { get; set; } = "";
	public string ShortName { get; set; } = "";
	public string Kind { get; set; } = "";
	public bool IsManaged { get; set; }
	public string? AssemblyName { get; set; }
	public string? ModuleName { get; set; }
	public int ModuleCount { get; set; }
}

public sealed class TypeListItem {
	public string FullName { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string Name { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool IsClass { get; set; }
	public bool IsInterface { get; set; }
	public bool IsEnum { get; set; }
	public bool IsValueType { get; set; }
	public string? BaseType { get; set; }
	public int MethodCount { get; set; }
	public int FieldCount { get; set; }
	public int PropertyCount { get; set; }
}

public sealed class TypeSearchResult {
	public string FullName { get; set; } = "";
	public string Assembly { get; set; } = "";
	public bool IsPublic { get; set; }
	public string Kind { get; set; } = "";
}

public sealed class TypeInfo {
	public string FullName { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string Name { get; set; } = "";
	public string Assembly { get; set; } = "";
	public string Kind { get; set; } = "";
	public string? BaseType { get; set; }
	public List<string> Interfaces { get; set; } = new();
	public bool IsAbstract { get; set; }
	public bool IsSealed { get; set; }
	public bool IsPublic { get; set; }
	public List<string> GenericParameters { get; set; } = new();
	public List<TypeMethodSummary> Methods { get; set; } = new();
	public List<TypeFieldSummary> Fields { get; set; } = new();
	public List<TypePropertySummary> Properties { get; set; } = new();
}

public sealed class TypeMethodSummary {
	public string Name { get; set; } = "";
	public string? ReturnType { get; set; }
	public List<string> Parameters { get; set; } = new();
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
	public bool IsConstructor { get; set; }
}

public sealed class TypeFieldSummary {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
	public bool IsPublic { get; set; }
	public bool IsStatic { get; set; }
}

public sealed class TypePropertySummary {
	public string Name { get; set; } = "";
	public string? Type { get; set; }
	public bool HasGetter { get; set; }
	public bool HasSetter { get; set; }
}

