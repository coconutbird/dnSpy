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

public sealed class UsageInfo {
	public string Location { get; set; } = "";
	public string UsageType { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class CallerInfo {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string FullSignature { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class CalleeInfo {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string FullSignature { get; set; } = "";
	public string? ILOffset { get; set; }
	public bool IsVirtual { get; set; }
}

public sealed class CallGraphNode {
	public string Id { get; set; } = "";
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string FullName { get; set; } = "";
	public int Depth { get; set; }
}

public sealed class CallGraphEdge {
	public string FromId { get; set; } = "";
	public string ToId { get; set; } = "";
	public string? ILOffset { get; set; }
}

public sealed class CallGraph {
	public List<CallGraphNode> Nodes { get; set; } = new();
	public List<CallGraphEdge> Edges { get; set; } = new();
	public int TotalNodes { get; set; }
	public int TotalEdges { get; set; }
	public bool Truncated { get; set; }
}

public sealed class TypeReferenceInfo {
	public string Location { get; set; } = "";
	public string ReferenceKind { get; set; } = "";
	public string Assembly { get; set; } = "";
}

public sealed class FieldReferenceInfo {
	public string Location { get; set; } = "";
	public string ReferenceType { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class StringUsageInfo {
	public string Value { get; set; } = "";
	public string Location { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class DependencyInfo {
	public string TypeName { get; set; } = "";
	public string Direction { get; set; } = "";
	public string DependencyKind { get; set; } = "";
	public string Assembly { get; set; } = "";
}

public sealed class DependencyAnalysis {
	public string TargetType { get; set; } = "";
	public List<DependencyInfo> Incoming { get; set; } = new();
	public List<DependencyInfo> Outgoing { get; set; } = new();
}

public sealed class TypeDependencyResult {
	public string TypeName { get; set; } = "";
	public List<string> OutgoingDependencies { get; set; } = new();
	public List<string> IncomingDependencies { get; set; } = new();
	public int OutgoingCount { get; set; }
	public int IncomingCount { get; set; }
}

public sealed class AssemblyDependencyResult {
	public string AssemblyName { get; set; } = "";
	public List<string> OutgoingDependencies { get; set; } = new();
	public List<string> IncomingDependencies { get; set; } = new();
	public int OutgoingCount { get; set; }
	public int IncomingCount { get; set; }
}

