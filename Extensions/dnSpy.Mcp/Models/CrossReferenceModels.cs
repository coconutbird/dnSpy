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
/// Information about a usage of a type, method, or field.
/// </summary>
public sealed class UsageInfo {
	/// <summary>Location where the usage occurs (e.g., "MyClass.MyMethod").</summary>
	public string Location { get; set; } = "";
	/// <summary>Type of usage (e.g., "Call", "NewObj", "FieldAccess").</summary>
	public string UsageType { get; set; } = "";
	/// <summary>IL offset of the usage instruction (e.g., "IL_0010").</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the usage.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a method that calls another method.
/// </summary>
public sealed class CallerInfo {
	/// <summary>Full name of the type containing the calling method.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the calling method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Full signature of the calling method.</summary>
	public string FullSignature { get; set; } = "";
	/// <summary>IL offset of the call instruction.</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the caller.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a method called by another method.
/// </summary>
public sealed class CalleeInfo {
	/// <summary>Full name of the type containing the called method.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the called method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Full signature of the called method.</summary>
	public string FullSignature { get; set; } = "";
	/// <summary>IL offset of the call instruction.</summary>
	public string? ILOffset { get; set; }
	/// <summary>True if this is a virtual call (callvirt instruction).</summary>
	public bool IsVirtual { get; set; }
}

/// <summary>
/// A node in a call graph representing a method.
/// </summary>
public sealed class CallGraphNode {
	/// <summary>Unique identifier for this node.</summary>
	public string Id { get; set; } = "";
	/// <summary>Full name of the type containing the method.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Full method name including type.</summary>
	public string FullName { get; set; } = "";
	/// <summary>Depth in the call graph from the root.</summary>
	public int Depth { get; set; }
}

/// <summary>
/// An edge in a call graph representing a call from one method to another.
/// </summary>
public sealed class CallGraphEdge {
	/// <summary>ID of the calling method node.</summary>
	public string FromId { get; set; } = "";
	/// <summary>ID of the called method node.</summary>
	public string ToId { get; set; } = "";
	/// <summary>IL offset of the call instruction.</summary>
	public string? ILOffset { get; set; }
}

/// <summary>
/// A call graph showing method call relationships.
/// </summary>
public sealed class CallGraph {
	/// <summary>Methods in the call graph.</summary>
	public List<CallGraphNode> Nodes { get; set; } = new();
	/// <summary>Call relationships between methods.</summary>
	public List<CallGraphEdge> Edges { get; set; } = new();
	/// <summary>Total number of nodes in the graph.</summary>
	public int TotalNodes { get; set; }
	/// <summary>Total number of edges in the graph.</summary>
	public int TotalEdges { get; set; }
	/// <summary>True if the graph was truncated due to size limits.</summary>
	public bool Truncated { get; set; }
}

/// <summary>
/// Information about a reference to a type.
/// </summary>
public sealed class TypeReferenceInfo {
	/// <summary>Location where the type is referenced.</summary>
	public string Location { get; set; } = "";
	/// <summary>Kind of reference (e.g., "BaseType", "FieldType", "MethodParameter").</summary>
	public string ReferenceKind { get; set; } = "";
	/// <summary>Assembly containing the reference.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a reference to a field.
/// </summary>
public sealed class FieldReferenceInfo {
	/// <summary>Location where the field is referenced.</summary>
	public string Location { get; set; } = "";
	/// <summary>Type of reference (e.g., "Load", "Store", "Address").</summary>
	public string ReferenceType { get; set; } = "";
	/// <summary>IL offset of the field access instruction.</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the reference.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a string literal usage.
/// </summary>
public sealed class StringUsageInfo {
	/// <summary>The string value being used.</summary>
	public string Value { get; set; } = "";
	/// <summary>Location where the string is used.</summary>
	public string Location { get; set; } = "";
	/// <summary>IL offset of the ldstr instruction.</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the string usage.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a dependency relationship between types.
/// </summary>
public sealed class DependencyInfo {
	/// <summary>Full name of the dependent or dependency type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Direction of dependency: "incoming" or "outgoing".</summary>
	public string Direction { get; set; } = "";
	/// <summary>Kind of dependency (e.g., "Inheritance", "FieldType", "MethodCall").</summary>
	public string DependencyKind { get; set; } = "";
	/// <summary>Assembly containing the related type.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Analysis of dependencies for a target type.
/// </summary>
public sealed class DependencyAnalysis {
	/// <summary>Full name of the analyzed type.</summary>
	public string TargetType { get; set; } = "";
	/// <summary>Types that depend on the target type.</summary>
	public List<DependencyInfo> Incoming { get; set; } = new();
	/// <summary>Types that the target type depends on.</summary>
	public List<DependencyInfo> Outgoing { get; set; } = new();
}

/// <summary>
/// Dependency analysis result for a type.
/// </summary>
public sealed class TypeDependencyResult {
	/// <summary>Full name of the analyzed type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Types that this type depends on.</summary>
	public List<string> OutgoingDependencies { get; set; } = new();
	/// <summary>Types that depend on this type.</summary>
	public List<string> IncomingDependencies { get; set; } = new();
	/// <summary>Total count of outgoing dependencies.</summary>
	public int OutgoingCount { get; set; }
	/// <summary>Total count of incoming dependencies.</summary>
	public int IncomingCount { get; set; }
}

/// <summary>
/// Dependency analysis result for an assembly.
/// </summary>
public sealed class AssemblyDependencyResult {
	/// <summary>Name of the analyzed assembly.</summary>
	public string AssemblyName { get; set; } = "";
	/// <summary>Assemblies that this assembly depends on.</summary>
	public List<string> OutgoingDependencies { get; set; } = new();
	/// <summary>Assemblies that depend on this assembly.</summary>
	public List<string> IncomingDependencies { get; set; } = new();
	/// <summary>Total count of outgoing dependencies.</summary>
	public int OutgoingCount { get; set; }
	/// <summary>Total count of incoming dependencies.</summary>
	public int IncomingCount { get; set; }
}

