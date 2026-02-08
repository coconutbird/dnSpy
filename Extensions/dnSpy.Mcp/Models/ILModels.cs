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

