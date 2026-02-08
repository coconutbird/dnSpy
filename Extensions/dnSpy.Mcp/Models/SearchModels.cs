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
/// Result from searching for string literals in IL code.
/// </summary>
public sealed class StringSearchResult {
	/// <summary>The string value found.</summary>
	public string Value { get; set; } = "";
	/// <summary>Full name of the type containing the string.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method using the string.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>IL offset of the ldstr instruction (e.g., "IL_0010").</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the string usage.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Result from searching for numeric constants in IL code.
/// </summary>
public sealed class NumberSearchResult {
	/// <summary>The numeric value found (as a string).</summary>
	public string Value { get; set; } = "";
	/// <summary>Type of the numeric value (e.g., "Int32", "Double").</summary>
	public string ValueType { get; set; } = "";
	/// <summary>Full name of the type containing the number.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method using the number.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>IL offset of the instruction loading the number.</summary>
	public string? ILOffset { get; set; }
	/// <summary>Assembly containing the number usage.</summary>
	public string Assembly { get; set; } = "";
}

#region Regex Search

/// <summary>
/// A single regex match in decompiled source code.
/// </summary>
public sealed class RegexMatchInfo {
	/// <summary>Full name of the type containing the match.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method containing the match.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Line number in the decompiled source.</summary>
	public int LineNumber { get; set; }
	/// <summary>The matched line of source code.</summary>
	public string MatchedLine { get; set; } = "";
	/// <summary>Assembly containing the match.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Regex matches grouped by type.
/// </summary>
public sealed class RegexTypeMatchInfo {
	/// <summary>Full name of the type containing matches.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Assembly containing the type.</summary>
	public string Assembly { get; set; } = "";
	/// <summary>Members containing matches.</summary>
	public List<RegexMemberMatchInfo> Members { get; set; } = new();
}

/// <summary>
/// Regex matches within a single member.
/// </summary>
public sealed class RegexMemberMatchInfo {
	/// <summary>Name of the member containing matches.</summary>
	public string MemberName { get; set; } = "";
	/// <summary>Kind of member: "Method", "Property", "Field", etc.</summary>
	public string MemberKind { get; set; } = "";
	/// <summary>Individual line matches within this member.</summary>
	public List<RegexLineMatchInfo> Matches { get; set; } = new();
}

/// <summary>
/// A single line match from a regex search.
/// </summary>
public sealed class RegexLineMatchInfo {
	/// <summary>Line number in the decompiled source.</summary>
	public int LineNumber { get; set; }
	/// <summary>The matched line of source code.</summary>
	public string Line { get; set; } = "";
}

#endregion

#region IL Pattern Search

/// <summary>
/// Result from searching for IL instruction patterns.
/// </summary>
public sealed class ILPatternMatchInfo {
	/// <summary>Full name of the type containing the match.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the method containing the match.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>IL offset where the pattern starts.</summary>
	public string ILOffset { get; set; } = "";
	/// <summary>The matched IL instructions.</summary>
	public List<string> MatchedInstructions { get; set; } = new();
	/// <summary>Assembly containing the match.</summary>
	public string Assembly { get; set; } = "";
}

#endregion

#region Attribute Search

/// <summary>
/// Result from searching for custom attributes.
/// </summary>
public sealed class AttributeMatchInfo {
	/// <summary>Kind of target: "Type", "Method", "Field", "Property", etc.</summary>
	public string TargetKind { get; set; } = "";
	/// <summary>Name of the attributed target.</summary>
	public string TargetName { get; set; } = "";
	/// <summary>Full name of the attribute type.</summary>
	public string AttributeType { get; set; } = "";
	/// <summary>Assembly containing the attributed item.</summary>
	public string Assembly { get; set; } = "";
}

#endregion

#region Signature Search

/// <summary>
/// Result from searching methods by signature pattern.
/// </summary>
public sealed class SignatureMatchInfo {
	/// <summary>Full name of the type containing the method.</summary>
	public string TypeName { get; set; } = "";
	/// <summary>Name of the matched method.</summary>
	public string MethodName { get; set; } = "";
	/// <summary>Return type of the method.</summary>
	public string ReturnType { get; set; } = "";
	/// <summary>Method parameters.</summary>
	public List<SignatureParameterInfo> Parameters { get; set; } = new();
	/// <summary>Full method signature string.</summary>
	public string FullSignature { get; set; } = "";
	/// <summary>Assembly containing the method.</summary>
	public string Assembly { get; set; } = "";
}

/// <summary>
/// Information about a method parameter in a signature search result.
/// </summary>
public sealed class SignatureParameterInfo {
	/// <summary>Parameter name.</summary>
	public string Name { get; set; } = "";
	/// <summary>Parameter type.</summary>
	public string Type { get; set; } = "";
}

#endregion

