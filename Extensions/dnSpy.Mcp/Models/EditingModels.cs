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

public sealed class EditILResult {
	public bool Success { get; set; }
	public string Method { get; set; } = "";
	public string Mode { get; set; } = "";
	public string Offset { get; set; } = "";
	public int InstructionsModified { get; set; }
	public string Message { get; set; } = "";
}

public sealed class PatchBytesResult {
	public bool Success { get; set; }
	public string Assembly { get; set; } = "";
	public string Offset { get; set; } = "";
	public int ByteCount { get; set; }
	public string Bytes { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class ReplaceMethodBodyResult {
	public bool Success { get; set; }
	public string Method { get; set; } = "";
	public int InstructionCount { get; set; }
	public int MaxStack { get; set; }
	public string Message { get; set; } = "";
}

public sealed class InjectCodeResult {
	public bool Success { get; set; }
	public string Method { get; set; } = "";
	public string Position { get; set; } = "";
	public int InstructionsInjected { get; set; }
	public string Message { get; set; } = "";
}

#endregion

#region Member Manipulation

public sealed class AddTypeResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string Kind { get; set; } = "";
	public string Token { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class AddMethodResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string Token { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class AddFieldResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string FieldName { get; set; } = "";
	public string Token { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class RemoveMemberResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string MemberName { get; set; } = "";
	public string MemberType { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class RenameMemberResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string OldName { get; set; } = "";
	public string NewName { get; set; } = "";
	public string MemberType { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class ChangeVisibilityResult {
	public bool Success { get; set; }
	public string TypeName { get; set; } = "";
	public string MemberName { get; set; } = "";
	public string MemberType { get; set; } = "";
	public string NewVisibility { get; set; } = "";
	public string Message { get; set; } = "";
}

public sealed class EditAttributeResult {
	public bool Success { get; set; }
	public string Target { get; set; } = "";
	public string AttributeType { get; set; } = "";
	public string Action { get; set; } = "";
	public string Message { get; set; } = "";
}

#endregion

