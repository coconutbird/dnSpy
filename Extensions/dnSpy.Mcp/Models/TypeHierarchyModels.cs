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
/// Type hierarchy information including base types and interfaces.
/// </summary>
public sealed class TypeHierarchy {
	/// <summary>Full name of the type being analyzed.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Chain of base types from immediate parent to System.Object.</summary>
	public List<string> BaseTypes { get; set; } = new();
	/// <summary>Interfaces directly implemented by this type.</summary>
	public List<string> Interfaces { get; set; } = new();
	/// <summary>All interfaces including inherited ones.</summary>
	public List<string> AllInterfaces { get; set; } = new();
}

/// <summary>
/// Information about a type that derives from or implements another type.
/// </summary>
public sealed class DerivedTypeInfo {
	/// <summary>Full name of the derived type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Assembly containing the derived type.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>True if this is a direct subclass/implementer, false if indirect.</summary>
	public bool IsDirect { get; set; }
	/// <summary>Type kind: "class", "struct", or "interface".</summary>
	public string Kind { get; set; } = "";
}

/// <summary>
/// Information about an interface implementation.
/// </summary>
public sealed class ImplementationInfo {
	/// <summary>Full name of the implementing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the implementing member.</summary>
	public string MemberName { get; set; } = "";
	/// <summary>Kind of member: "Method", "Property", or "Event".</summary>
	public string MemberKind { get; set; } = "";
	/// <summary>Assembly containing the implementation.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>True if this is an explicit interface implementation.</summary>
	public bool IsExplicit { get; set; }
}

