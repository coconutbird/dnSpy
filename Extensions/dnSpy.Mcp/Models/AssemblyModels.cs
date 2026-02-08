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
/// Comprehensive metadata about a .NET assembly.
/// </summary>
public sealed class AssemblyInfo {
	/// <summary>Simple name of the assembly (e.g., "System.Core").</summary>
	public string Name { get; set; } = "";
	/// <summary>Assembly version in Major.Minor.Build.Revision format.</summary>
	public string Version { get; set; } = "";
	/// <summary>Culture/locale of the assembly, or "neutral" if culture-invariant.</summary>
	public string Culture { get; set; } = "";
	/// <summary>Public key token as a hex string, or empty if not signed.</summary>
	public string PublicKeyToken { get; set; } = "";
	/// <summary>Full assembly name including version, culture, and public key token.</summary>
	public string FullName { get; set; } = "";
	/// <summary>Target framework (e.g., ".NETCoreApp,Version=v8.0").</summary>
	public string TargetFramework { get; set; } = "";
	/// <summary>Processor architecture (e.g., "x64", "x86", "AnyCPU").</summary>
	public string Architecture { get; set; } = "";
	/// <summary>Full name of the entry point method, or null if no entry point.</summary>
	public string? EntryPoint { get; set; }
	/// <summary>True if the assembly was compiled with debug information.</summary>
	public bool IsDebug { get; set; }
	/// <summary>True if the assembly has a strong name signature.</summary>
	public bool IsSigned { get; set; }
	/// <summary>Full file path to the assembly on disk.</summary>
	public string FilePath { get; set; } = "";
	/// <summary>Number of custom attributes applied to the assembly.</summary>
	public int CustomAttributeCount { get; set; }
	/// <summary>List of modules contained in this assembly.</summary>
	public List<ModuleInfo> Modules { get; set; } = new();
}

/// <summary>
/// Information about a module within an assembly.
/// </summary>
public sealed class ModuleInfo {
	/// <summary>Module name (typically the filename without path).</summary>
	public string? Name { get; set; }
	/// <summary>Module Version ID (MVID) - a unique GUID for this module build.</summary>
	public string? Mvid { get; set; }
	/// <summary>Number of types defined in this module.</summary>
	public int TypeCount { get; set; }
	/// <summary>True if this is the manifest module containing assembly metadata.</summary>
	public bool IsManifestModule { get; set; }
	/// <summary>CLR runtime version string (e.g., "v4.0.30319").</summary>
	public string? RuntimeVersion { get; set; }
	/// <summary>Full file path to the module.</summary>
	public string? Location { get; set; }
}

/// <summary>
/// Reference to an external assembly dependency.
/// </summary>
public sealed class AssemblyReference {
	/// <summary>Simple name of the referenced assembly.</summary>
	public string Name { get; set; } = "";
	/// <summary>Version of the referenced assembly.</summary>
	public string Version { get; set; } = "";
	/// <summary>Culture of the referenced assembly.</summary>
	public string Culture { get; set; } = "";
	/// <summary>Public key token of the referenced assembly.</summary>
	public string PublicKeyToken { get; set; } = "";
	/// <summary>Full assembly reference name.</summary>
	public string FullName { get; set; } = "";
}

/// <summary>
/// Custom attribute applied to an assembly.
/// </summary>
public sealed class AssemblyAttribute {
	/// <summary>Full type name of the attribute class.</summary>
	public string? AttributeType { get; set; }
	/// <summary>Constructor arguments passed to the attribute.</summary>
	public List<AttributeArgument> Arguments { get; set; } = new();
}

/// <summary>
/// A single argument passed to a custom attribute constructor.
/// </summary>
public sealed class AttributeArgument {
	/// <summary>Type of the argument value.</summary>
	public string? Type { get; set; }
	/// <summary>String representation of the argument value.</summary>
	public string? Value { get; set; }
}

/// <summary>
/// Summary of a namespace and its type count.
/// </summary>
public sealed class NamespaceInfo {
	/// <summary>Full namespace name (e.g., "System.Collections.Generic").</summary>
	public string Namespace { get; set; } = "";
	/// <summary>Number of types defined in this namespace.</summary>
	public int TypeCount { get; set; }
}

