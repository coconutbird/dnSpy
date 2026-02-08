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

#region IL Editing

/// <summary>
/// Result of an IL editing operation.
/// </summary>
public sealed class EditILResult {
	/// <summary>True if the edit was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the edited method.</summary>
	public string Method { get; set; } = "";
	/// <summary>Edit mode used: "Replace", "Insert", "Delete".</summary>
	public string Mode { get; set; } = "";
	/// <summary>IL offset where the edit occurred.</summary>
	public string Offset { get; set; } = "";
	/// <summary>Number of instructions affected.</summary>
	public int InstructionsModified { get; set; }
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of a raw byte patching operation.
/// </summary>
public sealed class PatchBytesResult {
	/// <summary>True if the patch was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Name of the patched assembly.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>File offset or RVA where bytes were patched.</summary>
	public string Offset { get; set; } = "";
	/// <summary>Number of bytes patched.</summary>
	public int ByteCount { get; set; }
	/// <summary>Hex representation of the patched bytes.</summary>
	public string Bytes { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of replacing an entire method body.
/// </summary>
public sealed class ReplaceMethodBodyResult {
	/// <summary>True if the replacement was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the affected method.</summary>
	public string Method { get; set; } = "";
	/// <summary>Number of instructions in the new body.</summary>
	public int InstructionCount { get; set; }
	/// <summary>Max stack size of the new body.</summary>
	public int MaxStack { get; set; }
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of injecting code into a method.
/// </summary>
public sealed class InjectCodeResult {
	/// <summary>True if the injection was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the affected method.</summary>
	public string Method { get; set; } = "";
	/// <summary>Position where code was injected: "Start", "End", or IL offset.</summary>
	public string Position { get; set; } = "";
	/// <summary>Number of instructions injected.</summary>
	public int InstructionsInjected { get; set; }
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

#endregion

#region Member Manipulation

/// <summary>
/// Result of adding a new type to an assembly.
/// </summary>
public sealed class AddTypeResult {
	/// <summary>True if the type was added successfully.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the new type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Kind of type: "class", "struct", "interface", "enum".</summary>
	public string Kind { get; set; } = "";
	/// <summary>Metadata token of the new type.</summary>
	public string Token { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of adding a new method to a type.
/// </summary>
public sealed class AddMethodResult {
	/// <summary>True if the method was added successfully.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the containing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the new method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Metadata token of the new method.</summary>
	public string Token { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of adding a new field to a type.
/// </summary>
public sealed class AddFieldResult {
	/// <summary>True if the field was added successfully.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the containing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the new field.</summary>
	public string FieldName { get; set; } = "";
	/// <summary>Metadata token of the new field.</summary>
	public string Token { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of removing a member from a type.
/// </summary>
public sealed class RemoveMemberResult {
	/// <summary>True if the member was removed successfully.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the containing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the removed member.</summary>
	public string MemberName { get; set; } = "";
	/// <summary>Type of member: "Method", "Field", "Property", "Event".</summary>
	public string MemberType { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of renaming a member.
/// </summary>
public sealed class RenameMemberResult {
	/// <summary>True if the rename was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the containing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Original member name.</summary>
	public string OldName { get; set; } = "";
	/// <summary>New member name.</summary>
	public string NewName { get; set; } = "";
	/// <summary>Type of member: "Method", "Field", "Property", "Event".</summary>
	public string MemberType { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of changing a member's visibility.
/// </summary>
public sealed class ChangeVisibilityResult {
	/// <summary>True if the visibility change was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Full name of the containing type.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the affected member.</summary>
	public string MemberName { get; set; } = "";
	/// <summary>Type of member: "Method", "Field", "Property", "Event", "Type".</summary>
	public string MemberType { get; set; } = "";
	/// <summary>New visibility: "public", "private", "protected", "internal".</summary>
	public string NewVisibility { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

/// <summary>
/// Result of adding, modifying, or removing a custom attribute.
/// </summary>
public sealed class EditAttributeResult {
	/// <summary>True if the attribute edit was successful.</summary>
	public bool Success { get; set; }
	/// <summary>Name of the attributed target.</summary>
	public string Target { get; set; } = "";
	/// <summary>Full name of the attribute type.</summary>
	public string AttributeType { get; set; } = "";
	/// <summary>Action performed: "Add", "Modify", "Remove".</summary>
	public string Action { get; set; } = "";
	/// <summary>Descriptive message about the operation.</summary>
	public string Message { get; set; } = "";
}

#endregion

