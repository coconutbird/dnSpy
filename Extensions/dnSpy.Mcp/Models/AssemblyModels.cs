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

public sealed class AssemblyInfo {
	public string Name { get; set; } = "";
	public string Version { get; set; } = "";
	public string Culture { get; set; } = "";
	public string PublicKeyToken { get; set; } = "";
	public string FullName { get; set; } = "";
	public string TargetFramework { get; set; } = "";
	public string Architecture { get; set; } = "";
	public string? EntryPoint { get; set; }
	public bool IsDebug { get; set; }
	public bool IsSigned { get; set; }
	public string FilePath { get; set; } = "";
	public int CustomAttributeCount { get; set; }
	public List<ModuleInfo> Modules { get; set; } = new();
}

public sealed class ModuleInfo {
	public string? Name { get; set; }
	public string? Mvid { get; set; }
	public int TypeCount { get; set; }
	public bool IsManifestModule { get; set; }
	public string? RuntimeVersion { get; set; }
	public string? Location { get; set; }
}

public sealed class AssemblyReference {
	public string Name { get; set; } = "";
	public string Version { get; set; } = "";
	public string Culture { get; set; } = "";
	public string PublicKeyToken { get; set; } = "";
	public string FullName { get; set; } = "";
}

public sealed class AssemblyAttribute {
	public string? AttributeType { get; set; }
	public List<AttributeArgument> Arguments { get; set; } = new();
}

public sealed class AttributeArgument {
	public string? Type { get; set; }
	public string? Value { get; set; }
}

public sealed class NamespaceInfo {
	public string Namespace { get; set; } = "";
	public int TypeCount { get; set; }
}

