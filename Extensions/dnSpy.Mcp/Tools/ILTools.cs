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

	[McpServerTool, Description("List metadata table contents")]
	public string ListMetadataTables(
		[Description("Assembly name")] string assemblyName,
		[Description("Table name: TypeDef, MethodDef, Field, MemberRef, TypeRef, etc.")] string table,
		[Description("Maximum rows to return (default: 100)")] int maxRows = 100,
		[Description("Starting row (default: 0)")] int offset = 0) {
		var doc = FindAssembly(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var module = doc.ModuleDef;
		var results = new List<object>();
		var rowIndex = 0;

		switch (table.ToLowerInvariant()) {
			case "typedef":
				foreach (var type in module.GetTypes().Skip(offset).Take(maxRows)) {
					results.Add(new TypeDefTableRow {
						Index = rowIndex++,
						Token = $"0x{type.MDToken.Raw:X8}",
						Name = type.Name.String,
						Namespace = type.Namespace.String,
						Flags = type.Attributes.ToString()
					});
				}
				break;

			case "methoddef":
				foreach (var type in module.GetTypes()) {
					foreach (var method in type.Methods.Skip(offset).Take(maxRows - results.Count)) {
						results.Add(new MethodDefTableRow {
							Index = rowIndex++,
							Token = $"0x{method.MDToken.Raw:X8}",
							Name = method.Name.String,
							RVA = method.RVA != 0 ? $"0x{method.RVA:X8}" : "0",
							ImplFlags = method.ImplAttributes.ToString(),
							Flags = method.Attributes.ToString()
						});
						if (results.Count >= maxRows) break;
					}
					if (results.Count >= maxRows) break;
				}
				break;

			case "field":
				foreach (var type in module.GetTypes()) {
					foreach (var field in type.Fields.Skip(offset).Take(maxRows - results.Count)) {
						results.Add(new FieldTableRow {
							Index = rowIndex++,
							Token = $"0x{field.MDToken.Raw:X8}",
							Name = field.Name.String,
							Flags = field.Attributes.ToString()
						});
						if (results.Count >= maxRows) break;
					}
					if (results.Count >= maxRows) break;
				}
				break;

			case "memberref":
				foreach (var mr in module.GetMemberRefs().Skip(offset).Take(maxRows)) {
					results.Add(new MemberRefTableRow {
						Index = rowIndex++,
						Token = $"0x{mr.MDToken.Raw:X8}",
						Name = mr.Name.String,
						Class = mr.Class?.ToString()
					});
				}
				break;

			case "typeref":
				foreach (var tr in module.GetTypeRefs().Skip(offset).Take(maxRows)) {
					results.Add(new TypeRefTableRow {
						Index = rowIndex++,
						Token = $"0x{tr.MDToken.Raw:X8}",
						Name = tr.Name.String,
						Namespace = tr.Namespace.String,
						ResolutionScope = tr.Scope?.ToString()
					});
				}
				break;

			case "assemblyref":
				foreach (var ar in module.GetAssemblyRefs().Skip(offset).Take(maxRows)) {
					results.Add(new AssemblyRefTableRow {
						Index = rowIndex++,
						Token = $"0x{ar.MDToken.Raw:X8}",
						Name = ar.Name.String,
						Version = ar.Version?.ToString() ?? "",
						Culture = ar.Culture.String
					});
				}
				break;

			case "customattribute":
				var attrs = new List<CustomAttributeTableRow>();
				var attrIndex = 0;
				foreach (var type in module.GetTypes()) {
					foreach (var attr in type.CustomAttributes) {
						attrs.Add(new CustomAttributeTableRow {
							Index = attrIndex++,
							Parent = type.FullName,
							AttributeType = attr.AttributeType?.FullName
						});
					}
				}
				results.AddRange(attrs.Skip(offset).Take(maxRows).Cast<object>());
				break;

			default:
				return JsonSerializer.Serialize(new ErrorResponse {
					Error = $"Unknown table '{table}'. Supported: TypeDef, MethodDef, Field, MemberRef, TypeRef, AssemblyRef, CustomAttribute"
				});
		}

		return JsonSerializer.Serialize(new MetadataTableResult {
			Table = table,
			Offset = $"0x{offset:X}",
			Count = results.Count,
			Rows = results
		}, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get PE header information")]
	public string GetPEInfo(
		[Description("Assembly name")] string assemblyName) {
		var doc = FindAssembly(assemblyName);
		if (doc is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var peImage = doc.PEImage;
		if (peImage is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' has no PE image" });

		var optHeader = peImage.ImageNTHeaders.OptionalHeader;
		var result = new PEInfo {
			Machine = peImage.ImageNTHeaders.FileHeader.Machine.ToString(),
			NumberOfSections = peImage.ImageNTHeaders.FileHeader.NumberOfSections,
			Timestamp = peImage.ImageNTHeaders.FileHeader.TimeDateStamp.ToString(),
			Characteristics = peImage.ImageNTHeaders.FileHeader.Characteristics.ToString(),
			Magic = optHeader.Magic.ToString(),
			Subsystem = optHeader.Subsystem.ToString(),
			ImageBase = $"0x{optHeader.ImageBase:X}",
			EntryPoint = $"0x{optHeader.AddressOfEntryPoint:X}",
			SectionAlignment = (int)optHeader.SectionAlignment,
			FileAlignment = (int)optHeader.FileAlignment,
			Sections = peImage.ImageSectionHeaders.Select(s => new PESectionInfo {
				Name = s.DisplayName,
				VirtualAddress = $"0x{s.VirtualAddress:X}",
				VirtualSize = $"0x{s.VirtualSize:X}",
				RawSize = $"0x{s.SizeOfRawData:X}",
				Characteristics = s.Characteristics.ToString()
			}).ToList()
		};

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get CLR header information")]
	public string GetCLRHeader(
		[Description("Assembly name")] string assemblyName) {
		var doc = FindAssembly(assemblyName);
		if (doc?.ModuleDef is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Assembly '{assemblyName}' not found" });

		var module = doc.ModuleDef;

		// Get COR20 header info from ModuleDef properties
		var runtimeVersion = module.Cor20HeaderRuntimeVersion;
		var majorVersion = (ushort)(runtimeVersion >> 16);
		var minorVersion = (ushort)(runtimeVersion & 0xFFFF);

		var result = new CLRHeaderInfo {
			HeaderSize = "72",
			MajorRuntimeVersion = majorVersion.ToString(),
			MinorRuntimeVersion = minorVersion.ToString(),
			Flags = module.Cor20HeaderFlags.ToString(),
			EntryPointToken = module.ManagedEntryPoint?.MDToken.Raw.ToString("X8") ?? "0",
			MetadataRVA = "N/A",
			MetadataSize = "N/A",
			ResourcesRVA = "N/A",
			ResourcesSize = "N/A"
		};

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	[McpServerTool, Description("Get type memory layout (for structs and classes with explicit layout)")]
	public string GetTypeLayout(
		[Description("Full type name")] string typeName) {
		TypeDef? type = null;
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			type = doc.ModuleDef.Find(typeName, true);
			if (type is not null) break;
		}

		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var fields = type.Fields
			.Where(f => !f.IsStatic)
			.Select(f => new FieldLayoutInfo {
				Name = f.Name.String,
				Type = f.FieldType?.FullName ?? "?",
				Offset = (int)(f.FieldOffset ?? 0),
				Size = GetFieldSize(f.FieldType) ?? 0
			})
			.OrderBy(f => f.Offset)
			.ToList();

		var result = new TypeLayoutInfo {
			TypeName = type.FullName,
			Size = (int)type.ClassSize,
			PackingSize = type.PackingSize,
			IsExplicitLayout = type.IsExplicitLayout,
			Fields = fields
		};

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	static int? GetFieldSize(TypeSig? typeSig) {
		if (typeSig is null) return null;

		// Handle primitive types
		switch (typeSig.ElementType) {
			case dnlib.DotNet.ElementType.Boolean:
			case dnlib.DotNet.ElementType.I1:
			case dnlib.DotNet.ElementType.U1:
				return 1;
			case dnlib.DotNet.ElementType.Char:
			case dnlib.DotNet.ElementType.I2:
			case dnlib.DotNet.ElementType.U2:
				return 2;
			case dnlib.DotNet.ElementType.I4:
			case dnlib.DotNet.ElementType.U4:
			case dnlib.DotNet.ElementType.R4:
				return 4;
			case dnlib.DotNet.ElementType.I8:
			case dnlib.DotNet.ElementType.U8:
			case dnlib.DotNet.ElementType.R8:
				return 8;
			case dnlib.DotNet.ElementType.I:
			case dnlib.DotNet.ElementType.U:
			case dnlib.DotNet.ElementType.Ptr:
			case dnlib.DotNet.ElementType.ByRef:
			case dnlib.DotNet.ElementType.Class:
			case dnlib.DotNet.ElementType.Object:
			case dnlib.DotNet.ElementType.String:
				return null; // Platform-dependent or reference type
			default:
				return null;
		}
	}

	[McpServerTool, Description("Get virtual method table (vtable) layout for a type")]
	public string ListVTable(
		[Description("Full type name")] string typeName) {
		TypeDef? type = null;
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.ModuleDef is null) continue;
			type = doc.ModuleDef.Find(typeName, true);
			if (type is not null) break;
		}

		if (type is null)
			return JsonSerializer.Serialize(new ErrorResponse { Error = $"Type '{typeName}' not found" });

		var vtable = new List<VTableEntry>();
		var slot = 0;

		// Collect virtual methods from base types first
		var typeHierarchy = new List<TypeDef>();
		var current = type;
		while (current is not null) {
			typeHierarchy.Insert(0, current);
			current = current.BaseType?.ResolveTypeDef();
		}

		var seenMethods = new HashSet<string>();

		foreach (var t in typeHierarchy) {
			foreach (var method in t.Methods.Where(m => m.IsVirtual)) {
				var sig = $"{method.Name}({string.Join(",", method.Parameters.Where(p => !p.IsHiddenThisParameter).Select(p => p.Type?.FullName))})";

				if (method.IsNewSlot || !seenMethods.Contains(sig)) {
					vtable.Add(new VTableEntry {
						Slot = slot++,
						MethodName = method.Name.String,
						DeclaringType = t.FullName,
						Signature = GetMethodSignature(method),
						IsAbstract = method.IsAbstract,
						IsFinal = method.IsFinal,
						IsNewSlot = method.IsNewSlot,
						IsOverride = false,
						Token = $"0x{method.MDToken.Raw:X8}"
					});
					seenMethods.Add(sig);
				}
				else {
					// Override - update existing slot
					var existingSlot = vtable.FindIndex(v => v.MethodName == method.Name.String);
					if (existingSlot >= 0) {
						vtable[existingSlot] = new VTableEntry {
							Slot = existingSlot,
							MethodName = method.Name.String,
							DeclaringType = t.FullName,
							Signature = GetMethodSignature(method),
							IsAbstract = method.IsAbstract,
							IsFinal = method.IsFinal,
							IsNewSlot = method.IsNewSlot,
							IsOverride = true,
							Token = $"0x{method.MDToken.Raw:X8}"
						};
					}
				}
			}
		}

		// Also include interface implementations
		var interfaces = new List<InterfaceImplementationInfo>();
		foreach (var iface in type.Interfaces) {
			var ifaceType = iface.Interface.ResolveTypeDef();
			if (ifaceType is null) continue;

			var ifaceMethods = ifaceType.Methods.Where(m => !m.IsStatic).Select(m => new InterfaceMethodInfo {
				InterfaceMethod = $"{ifaceType.FullName}.{m.Name}",
				Implementation = FindImplementation(type, ifaceType, m)
			}).ToList();

			interfaces.Add(new InterfaceImplementationInfo {
				InterfaceName = ifaceType.FullName,
				Methods = ifaceMethods
			});
		}

		var result = new VTableInfo {
			TypeName = type.FullName,
			VirtualMethods = vtable,
			InterfaceImplementations = interfaces
		};

		return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	static string? FindImplementation(TypeDef type, TypeDef iface, MethodDef ifaceMethod) {
		// Look for explicit implementation
		foreach (var method in type.Methods) {
			if (method.Overrides.Any(o => o.MethodDeclaration.Name == ifaceMethod.Name &&
				o.MethodDeclaration.DeclaringType.FullName == iface.FullName)) {
				return $"{type.FullName}.{method.Name}";
			}
		}

		// Look for implicit implementation
		var implMethod = type.Methods.FirstOrDefault(m =>
			m.Name == ifaceMethod.Name &&
			m.IsPublic &&
			!m.IsStatic &&
			ParametersMatch(m, ifaceMethod));

		return implMethod is not null ? $"{type.FullName}.{implMethod.Name}" : null;
	}

	static bool ParametersMatch(MethodDef m1, MethodDef m2) {
		var p1 = m1.Parameters.Where(p => !p.IsHiddenThisParameter).ToList();
		var p2 = m2.Parameters.Where(p => !p.IsHiddenThisParameter).ToList();

		if (p1.Count != p2.Count) return false;

		for (int i = 0; i < p1.Count; i++) {
			if (p1[i].Type?.FullName != p2[i].Type?.FullName)
				return false;
		}
		return true;
	}

	IDsDocument? FindAssembly(string name) {
		foreach (var doc in services.DocumentService.GetDocuments()) {
			if (doc.Filename.Contains(name, StringComparison.OrdinalIgnoreCase) ||
				(doc.AssemblyDef?.Name?.String?.Contains(name, StringComparison.OrdinalIgnoreCase) ?? false)) {
				return doc;
			}
		}
		return null;
	}
}
