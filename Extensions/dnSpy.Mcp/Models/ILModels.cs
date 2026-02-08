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

public sealed class ILInstruction {
	public int Offset { get; set; }
	public int Size { get; set; }
	public string OpCode { get; set; } = "";
	public string? Operand { get; set; }
	public string? OperandType { get; set; }
	public string? ResolvedOperand { get; set; }
}

public sealed class MethodBodyInfo {
	public int MaxStackSize { get; set; }
	public bool InitLocals { get; set; }
	public string? LocalVarSigToken { get; set; }
	public List<LocalVariableInfo> LocalVariables { get; set; } = new();
	public List<ExceptionHandlerInfo> ExceptionHandlers { get; set; } = new();
	public int CodeSize { get; set; }
}

public sealed class LocalVariableInfo {
	public int Index { get; set; }
	public string Type { get; set; } = "";
	public string? Name { get; set; }
	public bool IsPinned { get; set; }
}

public sealed class ExceptionHandlerInfo {
	public string HandlerType { get; set; } = "";
	public int TryStart { get; set; }
	public int TryEnd { get; set; }
	public int HandlerStart { get; set; }
	public int HandlerEnd { get; set; }
	public string? CatchType { get; set; }
	public int? FilterStart { get; set; }
}

public sealed class MethodBytesInfo {
	public string RVA { get; set; } = "";
	public int Size { get; set; }
	public string Bytes { get; set; } = "";
}

public sealed class TokenInfo {
	public string Token { get; set; } = "";
	public string TokenType { get; set; } = "";
	public string? Name { get; set; }
	public string? FullName { get; set; }
	public string? DeclaringType { get; set; }
	public string? Signature { get; set; }
}

#region Metadata Tables

public sealed class MetadataTableResult {
	public string Table { get; set; } = "";
	public string Offset { get; set; } = "";
	public int Count { get; set; }
	public List<object> Rows { get; set; } = new();
}

public sealed class TypeDefTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string Flags { get; set; } = "";
}

public sealed class MethodDefTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string RVA { get; set; } = "";
	public string ImplFlags { get; set; } = "";
	public string Flags { get; set; } = "";
}

public sealed class FieldTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string Flags { get; set; } = "";
}

public sealed class MemberRefTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Class { get; set; }
}

public sealed class TypeRefTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string? ResolutionScope { get; set; }
}

public sealed class AssemblyRefTableRow {
	public int Index { get; set; }
	public string Token { get; set; } = "";
	public string Name { get; set; } = "";
	public string Version { get; set; } = "";
	public string? Culture { get; set; }
}

public sealed class CustomAttributeTableRow {
	public int Index { get; set; }
	public string? Parent { get; set; }
	public string? AttributeType { get; set; }
}

#endregion

#region PE Info

public sealed class PEInfo {
	public string Machine { get; set; } = "";
	public int NumberOfSections { get; set; }
	public string Timestamp { get; set; } = "";
	public string Characteristics { get; set; } = "";
	public string Magic { get; set; } = "";
	public string Subsystem { get; set; } = "";
	public string ImageBase { get; set; } = "";
	public string EntryPoint { get; set; } = "";
	public int SectionAlignment { get; set; }
	public int FileAlignment { get; set; }
	public List<PESectionInfo> Sections { get; set; } = new();
}

public sealed class PESectionInfo {
	public string Name { get; set; } = "";
	public string VirtualAddress { get; set; } = "";
	public string VirtualSize { get; set; } = "";
	public string RawSize { get; set; } = "";
	public string Characteristics { get; set; } = "";
}

#endregion

#region CLR Header

public sealed class CLRHeaderInfo {
	public string HeaderSize { get; set; } = "";
	public string MajorRuntimeVersion { get; set; } = "";
	public string MinorRuntimeVersion { get; set; } = "";
	public string Flags { get; set; } = "";
	public string EntryPointToken { get; set; } = "";
	public string MetadataRVA { get; set; } = "";
	public string MetadataSize { get; set; } = "";
	public string ResourcesRVA { get; set; } = "";
	public string ResourcesSize { get; set; } = "";
}

#endregion

#region Type Layout

public sealed class TypeLayoutInfo {
	public string TypeName { get; set; } = "";
	public int Size { get; set; }
	public int PackingSize { get; set; }
	public bool IsExplicitLayout { get; set; }
	public List<FieldLayoutInfo> Fields { get; set; } = new();
}

public sealed class FieldLayoutInfo {
	public string Name { get; set; } = "";
	public string Type { get; set; } = "";
	public int Offset { get; set; }
	public int Size { get; set; }
}

#endregion

#region VTable

public sealed class VTableInfo {
	public string TypeName { get; set; } = "";
	public List<VTableEntry> VirtualMethods { get; set; } = new();
	public List<InterfaceImplementationInfo> InterfaceImplementations { get; set; } = new();
}

public sealed class VTableEntry {
	public int Slot { get; set; }
	public string MethodName { get; set; } = "";
	public string DeclaringType { get; set; } = "";
	public string Signature { get; set; } = "";
	public bool IsAbstract { get; set; }
	public bool IsFinal { get; set; }
	public bool IsNewSlot { get; set; }
	public bool IsOverride { get; set; }
	public string Token { get; set; } = "";
}

public sealed class InterfaceImplementationInfo {
	public string InterfaceName { get; set; } = "";
	public List<InterfaceMethodInfo> Methods { get; set; } = new();
}

public sealed class InterfaceMethodInfo {
	public string InterfaceMethod { get; set; } = "";
	public string? Implementation { get; set; }
}

#endregion

