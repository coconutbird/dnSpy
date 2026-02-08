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

using System.ComponentModel;
using System.Text.Json;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnSpy.Contracts.Documents;
using dnSpy.Mcp.Models;
using ModelContextProtocol.Server;

namespace dnSpy.Mcp.Tools;

/// <summary>
/// MCP tools for IL code analysis and low-level method inspection
/// </summary>
[McpServerToolType]
public sealed class ILTools {
	readonly DnSpyServices services;

	public ILTools(DnSpyServices services) {
		this.services = services;
	}

	[McpServerTool, Description("Get IL instructions for a method")]
	public string GetILInstructions(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Parameter signature for overload resolution (optional, e.g., 'int, string')")] string? signature = null,
		[Description("Maximum number of instructions to return (default: 500)")] int maxInstructions = 500) {
		var method = FindMethod(typeName, methodName, signature);
		if (method is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' not found" });

		var body = method.Body;
		if (body is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' has no body" });

		var instructions = body.Instructions
			.Take(maxInstructions)
			.Select(i => new ILInstruction {
				Offset = (int)i.Offset,
				Size = i.GetSize(),
				OpCode = i.OpCode.Name,
				Operand = FormatOperand(i.Operand),
				OperandType = i.OpCode.OperandType.ToString(),
				ResolvedOperand = ResolveOperand(i.Operand)
			}).ToList();

		return JsonSerializer.Serialize(instructions, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get method body metadata (locals, exception handlers, max stack)")]
	public string GetMethodBody(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Parameter signature for overload resolution (optional)")] string? signature = null) {
		var method = FindMethod(typeName, methodName, signature);
		if (method is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' not found" });

		var body = method.Body;
		if (body is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' has no body" });

		var info = new MethodBodyInfo {
			MaxStackSize = body.MaxStack,
			InitLocals = body.InitLocals,
			LocalVarSigToken = body.LocalVarSigTok != 0 ? $"0x{body.LocalVarSigTok:X8}" : null,
			CodeSize = body.Instructions.Sum(i => i.GetSize()),
			LocalVariables = body.Variables.Select((v, idx) => new Models.LocalVariableInfo {
				Index = idx,
				Type = v.Type?.FullName ?? "Unknown",
				Name = v.Name,
				IsPinned = v.Type is PinnedSig
			}).ToList(),
			ExceptionHandlers = body.ExceptionHandlers.Select(eh => new Models.ExceptionHandlerInfo {
				HandlerType = eh.HandlerType.ToString(),
				TryStart = (int)(eh.TryStart?.Offset ?? 0),
				TryEnd = (int)(eh.TryEnd?.Offset ?? 0),
				HandlerStart = (int)(eh.HandlerStart?.Offset ?? 0),
				HandlerEnd = (int)(eh.HandlerEnd?.Offset ?? 0),
				CatchType = eh.CatchType?.FullName,
				FilterStart = eh.FilterStart is not null ? (int?)eh.FilterStart.Offset : null
			}).ToList()
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get raw IL bytes for a method")]
	public string GetMethodBytes(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Method name")] string methodName,
		[Description("Parameter signature for overload resolution (optional)")] string? signature = null) {
		var method = FindMethod(typeName, methodName, signature);
		if (method is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' not found" });

		var body = method.Body;
		if (body is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Method '{typeName}.{methodName}' has no body" });

		var bytes = new List<byte>();
		foreach (var instr in body.Instructions) {
			if (instr.OpCode.Size == 1)
				bytes.Add((byte)instr.OpCode.Value);
			else {
				bytes.Add((byte)(instr.OpCode.Value >> 8));
				bytes.Add((byte)instr.OpCode.Value);
			}
			bytes.AddRange(GetOperandBytes(instr));
		}

		var info = new MethodBytesInfo {
			RVA = method.RVA != 0 ? $"0x{method.RVA:X8}" : "N/A",
			Size = bytes.Count,
			Bytes = BitConverter.ToString(bytes.ToArray()).Replace("-", " ")
		};

		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Resolve a metadata token to its definition")]
	public string ResolveToken(
		[Description("Assembly name (partial match supported)")] string assemblyName,
		[Description("Metadata token (hex, e.g., '0x06000001' or '06000001')")] string token) {
		var doc = FindDocument(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		if (!TryParseToken(token, out var tokenValue))
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Invalid token format: '{token}'" });

		var resolved = doc.ModuleDef.ResolveToken(tokenValue);
		if (resolved is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Token '{token}' could not be resolved" });

		var info = CreateTokenInfo(tokenValue, resolved);
		return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get the metadata token for a type, method, or field")]
	public string GetMetadataToken(
		[Description("Full type name (e.g., MyNamespace.MyClass)")] string typeName,
		[Description("Member name (optional - if omitted, returns type token)")] string? memberName = null) {
		var type = FindType(typeName);
		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		if (string.IsNullOrEmpty(memberName)) {
			var info = new TokenInfo {
				Token = $"0x{type.MDToken.Raw:X8}",
				TokenType = "TypeDef",
				Name = type.Name.String,
				FullName = type.FullName
			};
			return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
		}

		IMemberDef? member = type.FindMethod(memberName) as IMemberDef ??
			type.FindField(memberName) as IMemberDef ??
			type.Properties.FirstOrDefault(p => p.Name == memberName) as IMemberDef ??
			type.Events.FirstOrDefault(e => e.Name == memberName) as IMemberDef;

		if (member is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Member '{memberName}' not found in type '{typeName}'" });

		var memberInfo = CreateTokenInfo(member.MDToken.Raw, member);
		return JsonSerializer.Serialize(memberInfo, new JsonSerializerOptions { WriteIndented = true });
	}

	MethodDef? FindMethod(string typeName, string methodName, string? signature) {
		var type = FindType(typeName);
		if (type is null) return null;

		var methods = type.Methods.Where(m => m.Name == methodName).ToList();
		if (methods.Count == 0) return null;
		if (methods.Count == 1 || string.IsNullOrEmpty(signature)) return methods[0];

		var sigParts = signature.Split(',').Select(s => s.Trim()).ToArray();
		return methods.FirstOrDefault(m => MatchesSignature(m, sigParts)) ?? methods[0];
	}

	TypeDef? FindType(string typeName) {
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			var type = doc.ModuleDef.Find(typeName, false) ??
				doc.ModuleDef.Types.FirstOrDefault(t => t.FullName.EndsWith(typeName, StringComparison.OrdinalIgnoreCase));
			if (type is not null) return type;
		}
		return null;
	}

	IDsDocument? FindDocument(string name) {
		return services.DocumentService.GetDocuments().FirstOrDefault(doc =>
			doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
			(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false));
	}

	static bool MatchesSignature(MethodDef method, string[] sigParts) {
		var parameters = method.Parameters.Where(p => !p.IsHiddenThisParameter).ToList();
		if (parameters.Count != sigParts.Length) return false;
		for (int i = 0; i < sigParts.Length; i++) {
			var paramType = parameters[i].Type?.TypeName ?? "";
			if (!paramType.Contains(sigParts[i], StringComparison.OrdinalIgnoreCase)) return false;
		}
		return true;
	}

	static string? FormatOperand(object? operand) => operand switch {
		null => null,
		Instruction instr => $"IL_{instr.Offset:X4}",
		Instruction[] instrs => string.Join(", ", instrs.Select(i => $"IL_{i.Offset:X4}")),
		Local local => $"V_{local.Index}",
		Parameter param => $"A_{param.Index}",
		IField field => field.FullName,
		IMethod method => method.FullName,
		ITypeDefOrRef type => type.FullName,
		string s => $"\"{s}\"",
		_ => operand.ToString()
	};

	static string? ResolveOperand(object? operand) => operand switch {
		MemberRef mr => $"MemberRef: {mr.FullName}",
		IField field => $"Field: {field.FullName}",
		IMethod method => $"Method: {method.FullName}",
		ITypeDefOrRef type => $"Type: {type.FullName}",
		_ => null
	};

	static byte[] GetOperandBytes(Instruction instr) {
		// Simplified - actual operand encoding depends on OpCode.OperandType
		return instr.OpCode.OperandType switch {
			OperandType.InlineNone => Array.Empty<byte>(),
			OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar =>
				new byte[] { (byte)(instr.Operand is Instruction target ? (sbyte)(target.Offset - instr.Offset - instr.GetSize()) : 0) },
			OperandType.InlineI => BitConverter.GetBytes(instr.Operand is int i ? i : 0),
			OperandType.InlineI8 => BitConverter.GetBytes(instr.Operand is long l ? l : 0L),
			OperandType.ShortInlineR => BitConverter.GetBytes(instr.Operand is float f ? f : 0f),
			OperandType.InlineR => BitConverter.GetBytes(instr.Operand is double d ? d : 0d),
			_ => BitConverter.GetBytes(instr.Operand is IMDTokenProvider token ? (int)token.MDToken.Raw : 0)
		};
	}

	static bool TryParseToken(string token, out uint value) {
		var s = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? token[2..] : token;
		return uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out value);
	}

	static TokenInfo CreateTokenInfo(uint token, object resolved) {
		var info = new TokenInfo { Token = $"0x{token:X8}" };
		switch (resolved) {
			case TypeDef td:
				info.TokenType = "TypeDef";
				info.Name = td.Name.String;
				info.FullName = td.FullName;
				break;
			case MethodDef md:
				info.TokenType = "MethodDef";
				info.Name = md.Name.String;
				info.FullName = md.FullName;
				info.DeclaringType = md.DeclaringType?.FullName;
				info.Signature = GetMethodSignature(md);
				break;
			case FieldDef fd:
				info.TokenType = "FieldDef";
				info.Name = fd.Name.String;
				info.FullName = fd.FullName;
				info.DeclaringType = fd.DeclaringType?.FullName;
				break;
			case MemberRef mr:
				info.TokenType = "MemberRef";
				info.Name = mr.Name.String;
				info.FullName = mr.FullName;
				break;
			default:
				info.TokenType = resolved.GetType().Name;
				info.Name = resolved.ToString();
				break;
		}
		return info;
	}

	static string GetMethodSignature(MethodDef method) {
		var parameters = method.Parameters
			.Where(p => !p.IsHiddenThisParameter)
			.Select(p => $"{p.Type?.TypeName ?? "?"} {p.Name}");
		return $"{method.ReturnType?.TypeName ?? "void"} {method.Name}({string.Join(", ", parameters)})";
	}
}
