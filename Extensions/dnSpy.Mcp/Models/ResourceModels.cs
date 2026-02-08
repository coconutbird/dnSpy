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

public sealed class ResourceListItem {
	public string Name { get; set; } = "";
	public string Type { get; set; } = "";
	public int Size { get; set; }
	public bool IsPublic { get; set; }
	public bool IsEmbedded { get; set; }
	public bool IsLinked { get; set; }
}

public sealed class ResourceSetInfo {
	public string Name { get; set; } = "";
	public int EntryCount { get; set; }
	public string ResourceType { get; set; } = "";
}

public sealed class ResourceContent {
	public string Name { get; set; } = "";
	public string Content { get; set; } = "";
	public int Size { get; set; }
	public string Encoding { get; set; } = "";
	public string? MimeType { get; set; }
}

public sealed class ResourceInfo {
	public string Name { get; set; } = "";
	public string Type { get; set; } = "";
	public int Size { get; set; }
	public bool IsPublic { get; set; }
	public string ResourceKind { get; set; } = "";
	public string? Culture { get; set; }
}

public sealed class ResourceEntry {
	public string Key { get; set; } = "";
	public string Type { get; set; } = "";
	public int Size { get; set; }
	public string? Value { get; set; }
}

