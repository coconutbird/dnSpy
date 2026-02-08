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
/// A single IL (Intermediate Language) instruction.
/// </summary>
public sealed class ILInstruction {
	/// <summary>Byte offset of this instruction from the start of the method body.</summary>
	public int Offset { get; set; }
	/// <summary>Size of this instruction in bytes.</summary>
	public int Size { get; set; }
	/// <summary>IL opcode name (e.g., "ldarg.0", "call", "ret").</summary>
	public string OpCode { get; set; } = "";
	/// <summary>Raw operand value as a string, if any.</summary>
	public string? Operand { get; set; }
	/// <summary>Type of operand (e.g., "InlineMethod", "InlineString", "ShortInlineBrTarget").</summary>
	public string? OperandType { get; set; }
	/// <summary>Human-readable resolved operand (e.g., method name, type name).</summary>
	public string? ResolvedOperand { get; set; }
}

/// <summary>
/// Metadata about a method body including locals and exception handlers.
/// </summary>
public sealed class MethodBodyInfo {
	/// <summary>Maximum number of items on the evaluation stack.</summary>
	public int MaxStackSize { get; set; }
	/// <summary>True if local variables are initialized to default values.</summary>
	public bool InitLocals { get; set; }
	/// <summary>Metadata token of the local variable signature, if any.</summary>
	public string? LocalVarSigToken { get; set; }
	/// <summary>Local variables declared in this method.</summary>
	public List<LocalVariableInfo> LocalVariables { get; set; } = new();
	/// <summary>Exception handlers (try/catch/finally blocks).</summary>
	public List<ExceptionHandlerInfo> ExceptionHandlers { get; set; } = new();
	/// <summary>Size of the IL code in bytes.</summary>
	public int CodeSize { get; set; }
}

/// <summary>
/// Information about a local variable in a method.
/// </summary>
public sealed class LocalVariableInfo {
	/// <summary>Zero-based index of the local variable.</summary>
	public int Index { get; set; }
	/// <summary>Type of the local variable.</summary>
	public string Type { get; set; } = "";
	/// <summary>Debug name of the local variable, if available from PDB.</summary>
	public string? Name { get; set; }
	/// <summary>True if this is a pinned local (for fixed statements).</summary>
	public bool IsPinned { get; set; }
}

/// <summary>
/// Information about an exception handler (try/catch/finally/filter block).
/// </summary>
public sealed class ExceptionHandlerInfo {
	/// <summary>Handler type: "Catch", "Finally", "Filter", or "Fault".</summary>
	public string HandlerType { get; set; } = "";
	/// <summary>IL offset where the try block starts.</summary>
	public int TryStart { get; set; }
	/// <summary>IL offset where the try block ends.</summary>
	public int TryEnd { get; set; }
	/// <summary>IL offset where the handler block starts.</summary>
	public int HandlerStart { get; set; }
	/// <summary>IL offset where the handler block ends.</summary>
	public int HandlerEnd { get; set; }
	/// <summary>Type being caught (for Catch handlers), null otherwise.</summary>
	public string? CatchType { get; set; }
	/// <summary>IL offset where the filter expression starts (for Filter handlers).</summary>
	public int? FilterStart { get; set; }
}

/// <summary>
/// Raw bytes of a method body.
/// </summary>
public sealed class MethodBytesInfo {
	/// <summary>Relative Virtual Address of the method body in hex format (e.g., "0x00002050").</summary>
	public string RVA { get; set; } = "";
	/// <summary>Size of the method body in bytes.</summary>
	public int Size { get; set; }
	/// <summary>Hex-encoded bytes of the method body.</summary>
	public string Bytes { get; set; } = "";
}

/// <summary>
/// Information about a metadata token.
/// </summary>
public sealed class TokenInfo {
	/// <summary>Metadata token in hex format (e.g., "0x06000001").</summary>
	public string Token { get; set; } = "";
	/// <summary>Token type (e.g., "TypeDef", "MethodDef", "MemberRef").</summary>
	public string TokenType { get; set; } = "";
	/// <summary>Simple name of the referenced item.</summary>
	public string? Name { get; set; }
	/// <summary>Full name including namespace/type.</summary>
	public string? FullName { get; set; }
	/// <summary>Declaring type for members.</summary>
	public string? DeclaringType { get; set; }
	/// <summary>Method/field signature for member references.</summary>
	public string? Signature { get; set; }
}

#region Metadata Tables

/// <summary>
/// Result from reading a metadata table.
/// </summary>
public sealed class MetadataTableResult {
	/// <summary>Name of the metadata table (e.g., "TypeDef", "MethodDef").</summary>
	public string Table { get; set; } = "";
	/// <summary>File offset of the table in hex format.</summary>
	public string Offset { get; set; } = "";
	/// <summary>Number of rows in the table.</summary>
	public int Count { get; set; }
	/// <summary>Table rows (type depends on the specific table).</summary>
	public List<object> Rows { get; set; } = new();
}

/// <summary>
/// A row from the TypeDef metadata table.
/// </summary>
public sealed class TypeDefTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Type name without namespace.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type namespace.</summary>
	public string Namespace { get; set; } = "";
	/// <summary>Type attribute flags.</summary>
	public string Flags { get; set; } = "";
}

/// <summary>
/// A row from the MethodDef metadata table.
/// </summary>
public sealed class MethodDefTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Method name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Relative Virtual Address of the method body.</summary>
	public string RVA { get; set; } = "";
	/// <summary>Method implementation flags.</summary>
	public string ImplFlags { get; set; } = "";
	/// <summary>Method attribute flags.</summary>
	public string Flags { get; set; } = "";
}

/// <summary>
/// A row from the Field metadata table.
/// </summary>
public sealed class FieldTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Field name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Field attribute flags.</summary>
	public string Flags { get; set; } = "";
}

/// <summary>
/// A row from the MemberRef metadata table.
/// </summary>
public sealed class MemberRefTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Member name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Class or type containing the member.</summary>
	public string? Class { get; set; }
}

/// <summary>
/// A row from the TypeRef metadata table.
/// </summary>
public sealed class TypeRefTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Type name without namespace.</summary>
	public string Name { get; set; } = "";
	/// <summary>Type namespace.</summary>
	public string Namespace { get; set; } = "";
	/// <summary>Resolution scope (assembly or module reference).</summary>
	public string? ResolutionScope { get; set; }
}

/// <summary>
/// A row from the AssemblyRef metadata table.
/// </summary>
public sealed class AssemblyRefTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
	/// <summary>Assembly name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Assembly version.</summary>
	public string Version { get; set; } = "";
	/// <summary>Assembly culture.</summary>
	public string? Culture { get; set; }
}

/// <summary>
/// A row from the CustomAttribute metadata table.
/// </summary>
public sealed class CustomAttributeTableRow {
	/// <summary>Zero-based row index in the table.</summary>
	public int Index { get; set; }
	/// <summary>Parent item the attribute is applied to.</summary>
	public string? Parent { get; set; }
	/// <summary>Type of the custom attribute.</summary>
	public string? AttributeType { get; set; }
}

#endregion

#region PE Info

/// <summary>
/// Portable Executable (PE) file header information.
/// </summary>
public sealed class PEInfo {
	/// <summary>Target machine architecture (e.g., "I386", "AMD64").</summary>
	public string Machine { get; set; } = "";
	/// <summary>Number of sections in the PE file.</summary>
	public int NumberOfSections { get; set; }
	/// <summary>Build timestamp of the PE file.</summary>
	public string Timestamp { get; set; } = "";
	/// <summary>PE characteristics flags.</summary>
	public string Characteristics { get; set; } = "";
	/// <summary>PE magic number (PE32 or PE32+).</summary>
	public string Magic { get; set; } = "";
	/// <summary>Windows subsystem (e.g., "WindowsGui", "WindowsCui").</summary>
	public string Subsystem { get; set; } = "";
	/// <summary>Preferred base address in hex format.</summary>
	public string ImageBase { get; set; } = "";
	/// <summary>Entry point RVA in hex format.</summary>
	public string EntryPoint { get; set; } = "";
	/// <summary>Section alignment in memory.</summary>
	public int SectionAlignment { get; set; }
	/// <summary>Section alignment in file.</summary>
	public int FileAlignment { get; set; }
	/// <summary>PE sections.</summary>
	public List<PESectionInfo> Sections { get; set; } = new();
}

/// <summary>
/// Information about a PE section.
/// </summary>
public sealed class PESectionInfo {
	/// <summary>Section name (e.g., ".text", ".data", ".rsrc").</summary>
	public string Name { get; set; } = "";
	/// <summary>Virtual address in memory (hex format).</summary>
	public string VirtualAddress { get; set; } = "";
	/// <summary>Size in memory (hex format).</summary>
	public string VirtualSize { get; set; } = "";
	/// <summary>Size on disk (hex format).</summary>
	public string RawSize { get; set; } = "";
	/// <summary>Section characteristics flags.</summary>
	public string Characteristics { get; set; } = "";
}

#endregion

#region CLR Header

/// <summary>
/// CLR (Common Language Runtime) header information for managed assemblies.
/// </summary>
public sealed class CLRHeaderInfo {
	/// <summary>Size of the CLR header.</summary>
	public string HeaderSize { get; set; } = "";
	/// <summary>Major runtime version required.</summary>
	public string MajorRuntimeVersion { get; set; } = "";
	/// <summary>Minor runtime version required.</summary>
	public string MinorRuntimeVersion { get; set; } = "";
	/// <summary>CLR flags (e.g., ILOnly, Required32Bit).</summary>
	public string Flags { get; set; } = "";
	/// <summary>Entry point method token.</summary>
	public string EntryPointToken { get; set; } = "";
	/// <summary>RVA of the metadata section.</summary>
	public string MetadataRVA { get; set; } = "";
	/// <summary>Size of the metadata section.</summary>
	public string MetadataSize { get; set; } = "";
	/// <summary>RVA of the resources section.</summary>
	public string ResourcesRVA { get; set; } = "";
	/// <summary>Size of the resources section.</summary>
	public string ResourcesSize { get; set; } = "";
}

#endregion

#region Type Layout

/// <summary>
/// Memory layout information for a type.
/// </summary>
public sealed class TypeLayoutInfo {
	/// <summary>Full type name.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Total size of the type in bytes.</summary>
	public int Size { get; set; }
	/// <summary>Packing size for field alignment.</summary>
	public int PackingSize { get; set; }
	/// <summary>True if fields have explicit offsets.</summary>
	public bool IsExplicitLayout { get; set; }
	/// <summary>Field layout information.</summary>
	public List<FieldLayoutInfo> Fields { get; set; } = new();
}

/// <summary>
/// Memory layout information for a field.
/// </summary>
public sealed class FieldLayoutInfo {
	/// <summary>Field name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Field type.</summary>
	public string Type { get; set; } = "";
	/// <summary>Byte offset from the start of the type.</summary>
	public int Offset { get; set; }
	/// <summary>Size of the field in bytes.</summary>
	public int Size { get; set; }
}

#endregion

#region VTable

/// <summary>
/// Virtual method table (vtable) information for a type.
/// </summary>
public sealed class VTableInfo {
	/// <summary>Full type name.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Virtual methods in the vtable.</summary>
	public List<VTableEntry> VirtualMethods { get; set; } = new();
	/// <summary>Interface implementations.</summary>
	public List<InterfaceImplementationInfo> InterfaceImplementations { get; set; } = new();
}

/// <summary>
/// An entry in the virtual method table.
/// </summary>
public sealed class VTableEntry {
	/// <summary>Slot number in the vtable.</summary>
	public int Slot { get; set; }
	/// <summary>Method name.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Type that declares this method.</summary>
	public string DeclaringType { get; set; } = "";
	/// <summary>Full method signature.</summary>
	public string Signature { get; set; } = "";
	/// <summary>True if the method is abstract.</summary>
	public bool IsAbstract { get; set; }
	/// <summary>True if the method is sealed (final).</summary>
	public bool IsFinal { get; set; }
	/// <summary>True if this method creates a new vtable slot.</summary>
	public bool IsNewSlot { get; set; }
	/// <summary>True if this method overrides a base method.</summary>
	public bool IsOverride { get; set; }
	/// <summary>Metadata token in hex format.</summary>
	public string Token { get; set; } = "";
}

/// <summary>
/// Information about an interface implementation.
/// </summary>
public sealed class InterfaceImplementationInfo {
	/// <summary>Full name of the implemented interface.</summary>
	public string InterfaceName { get; set; } = "";
	/// <summary>Methods implementing the interface.</summary>
	public List<InterfaceMethodInfo> Methods { get; set; } = new();
}

/// <summary>
/// Mapping between an interface method and its implementation.
/// </summary>
public sealed class InterfaceMethodInfo {
	/// <summary>Interface method signature.</summary>
	public string InterfaceMethod { get; set; } = "";
	/// <summary>Implementing method name, or null if not implemented.</summary>
	public string? Implementation { get; set; }
}

#endregion

