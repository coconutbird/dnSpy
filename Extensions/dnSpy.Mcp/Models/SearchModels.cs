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

public sealed class StringSearchResult {
	public string Value { get; set; } = "";
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

public sealed class NumberSearchResult {
	public string Value { get; set; } = "";
	public string ValueType { get; set; } = "";
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string? ILOffset { get; set; }
	public string Assembly { get; set; } = "";
}

#region Regex Search

public sealed class RegexMatchInfo {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public int LineNumber { get; set; }
	public string MatchedLine { get; set; } = "";
	public string Assembly { get; set; } = "";
}

public sealed class RegexTypeMatchInfo {
	public string TypeName { get; set; } = "";
	public string Assembly { get; set; } = "";
	public List<RegexMemberMatchInfo> Members { get; set; } = new();
}

public sealed class RegexMemberMatchInfo {
	public string MemberName { get; set; } = "";
	public string MemberKind { get; set; } = "";
	public List<RegexLineMatchInfo> Matches { get; set; } = new();
}

public sealed class RegexLineMatchInfo {
	public int LineNumber { get; set; }
	public string Line { get; set; } = "";
}

#endregion

#region IL Pattern Search

public sealed class ILPatternMatchInfo {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string ILOffset { get; set; } = "";
	public List<string> MatchedInstructions { get; set; } = new();
	public string Assembly { get; set; } = "";
}

#endregion

#region Attribute Search

public sealed class AttributeMatchInfo {
	public string TargetKind { get; set; } = "";
	public string TargetName { get; set; } = "";
	public string AttributeType { get; set; } = "";
	public string Assembly { get; set; } = "";
}

#endregion

#region Signature Search

public sealed class SignatureMatchInfo {
	public string TypeName { get; set; } = "";
	public string MethodName { get; set; } = "";
	public string ReturnType { get; set; } = "";
	public List<SignatureParameterInfo> Parameters { get; set; } = new();
	public string FullSignature { get; set; } = "";
	public string Assembly { get; set; } = "";
}

public sealed class SignatureParameterInfo {
	public string Name { get; set; } = "";
	public string Type { get; set; } = "";
}

#endregion

