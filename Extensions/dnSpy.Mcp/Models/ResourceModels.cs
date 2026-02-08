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
/// Summary information about a resource in an assembly.
/// </summary>
public sealed class ResourceListItem {
	/// <summary>Name of the resource.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the resource (e.g., "Embedded", "Linked", "ResourceSet").</summary>
	public string Type { get; set; } = "";
	/// <summary>Size of the resource in bytes.</summary>
	public int Size { get; set; }
	/// <summary>True if the resource is publicly visible.</summary>
	public bool IsPublic { get; set; }
	/// <summary>True if the resource is embedded in the assembly.</summary>
	public bool IsEmbedded { get; set; }
	/// <summary>True if the resource is linked (external file).</summary>
	public bool IsLinked { get; set; }
}

/// <summary>
/// Information about a .resx resource set.
/// </summary>
public sealed class ResourceSetInfo {
	/// <summary>Name of the resource set.</summary>
	public string Name { get; set; } = "";
	/// <summary>Number of entries in the resource set.</summary>
	public int EntryCount { get; set; }
	/// <summary>Type of the resource set (e.g., "ResourceManager").</summary>
	public string ResourceType { get; set; } = "";
}

/// <summary>
/// Content of an extracted resource.
/// </summary>
public sealed class ResourceContent {
	/// <summary>Name of the resource.</summary>
	public string Name { get; set; } = "";
	/// <summary>Resource content (base64-encoded for binary data).</summary>
	public string Content { get; set; } = "";
	/// <summary>Size of the resource in bytes.</summary>
	public int Size { get; set; }
	/// <summary>Encoding used: "utf-8", "base64", etc.</summary>
	public string Encoding { get; set; } = "";
	/// <summary>MIME type of the content if known.</summary>
	public string? MimeType { get; set; }
}

/// <summary>
/// Detailed information about a resource.
/// </summary>
public sealed class ResourceInfo {
	/// <summary>Name of the resource.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type of the resource content.</summary>
	public string Type { get; set; } = "";
	/// <summary>Size of the resource in bytes.</summary>
	public int Size { get; set; }
	/// <summary>True if the resource is publicly visible.</summary>
	public bool IsPublic { get; set; }
	/// <summary>Kind of resource: "Embedded", "Linked", "AssemblyLinked".</summary>
	public string ResourceKind { get; set; } = "";
	/// <summary>Culture/locale for satellite assemblies.</summary>
	public string? Culture { get; set; }
}

/// <summary>
/// An entry within a resource set.
/// </summary>
public sealed class ResourceEntry {
	/// <summary>Key/name of the entry.</summary>
	public string Key { get; set; } = "";
	/// <summary>Type of the entry value.</summary>
	public string Type { get; set; } = "";
	/// <summary>Size of the entry in bytes.</summary>
	public int Size { get; set; }
	/// <summary>String representation of the value (may be truncated for large values).</summary>
	public string? Value { get; set; }
}

