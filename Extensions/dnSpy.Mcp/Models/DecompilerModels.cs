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
/// Information about an available decompiler.
/// </summary>
public sealed class DecompilerInfo {
	/// <summary>Display name of the decompiler (e.g., "C# 12.0").</summary>
	public string Name { get; set; } = "";
	/// <summary>Generic name of the language (e.g., "C#", "VB", "IL").</summary>
	public string GenericName { get; set; } = "";
	/// <summary>File extension for decompiled output (e.g., ".cs", ".vb").</summary>
	public string FileExtension { get; set; } = "";
	/// <summary>True if this is the currently selected decompiler.</summary>
	public bool IsCurrent { get; set; }
}

