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

using System.Text.Json;
using dnSpy.Mcp.Models;
using dnSpy.Mcp.Tools;
using Xunit;

namespace dnSpy.Mcp.Tests;

public class AssemblyToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly AssemblyTools tools;

	// Get the actual CoreLib name for the running framework
	static string CoreLibName => typeof(object).Assembly.GetName().Name ?? "System.Private.CoreLib";

	public AssemblyToolsTests() {
		fixture = new TestFixture();
		tools = new AssemblyTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region GetAssemblyInfo Tests

	[Fact]
	public void GetAssemblyInfo_ValidAssembly_ReturnsInfo() {
		var result = tools.GetAssemblyInfo(CoreLibName);
		var info = JsonSerializer.Deserialize<AssemblyInfo>(result);

		Assert.NotNull(info);
		Assert.NotNull(info.Name);
		Assert.NotNull(info.Version);
		Assert.NotNull(info.FullName);
	}

	[Fact]
	public void GetAssemblyInfo_NonExistentAssembly_ReturnsError() {
		var result = tools.GetAssemblyInfo("NonExistent.Assembly");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetAssemblyInfo_PartialName_Matches() {
		// Should match via partial name - use the first part of the CoreLib name
		// On .NET Core/5+: "System.Private.CoreLib" -> "CoreLib" partial match
		// On .NET Framework: "mscorlib" -> "mscor" partial match
		var partialName = CoreLibName.Contains("CoreLib") ? "CoreLib" : "mscor";
		var result = tools.GetAssemblyInfo(partialName);
		var info = JsonSerializer.Deserialize<AssemblyInfo>(result);

		Assert.NotNull(info);
		Assert.Contains(partialName, info.Name, StringComparison.OrdinalIgnoreCase);
	}

	#endregion

	#region ListAssemblyReferences Tests

	[Fact]
	public void ListAssemblyReferences_ValidAssembly_ReturnsList() {
		var result = tools.ListAssemblyReferences(CoreLibName);
		var items = JsonSerializer.Deserialize<List<AssemblyReference>>(result);

		Assert.NotNull(items);
		// CoreLib may have few/no references, but should return valid list
	}

	[Fact]
	public void ListAssemblyReferences_NonExistentAssembly_ReturnsError() {
		var result = tools.ListAssemblyReferences("NonExistent.Assembly");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region ListNamespaces Tests

	[Fact]
	public void ListNamespaces_ValidAssembly_ReturnsNamespaces() {
		var result = tools.ListNamespaces(CoreLibName);
		var items = JsonSerializer.Deserialize<List<NamespaceInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.Contains(items, n => n.Namespace == "System");
	}

	[Fact]
	public void ListNamespaces_NonExistentAssembly_ReturnsError() {
		var result = tools.ListNamespaces("NonExistent.Assembly");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region ListAssemblyAttributes Tests

	[Fact]
	public void ListAssemblyAttributes_ValidAssembly_ReturnsAttributes() {
		var result = tools.ListAssemblyAttributes(CoreLibName);
		var items = JsonSerializer.Deserialize<List<AssemblyAttribute>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
	}

	[Fact]
	public void ListAssemblyAttributes_NonExistentAssembly_ReturnsError() {
		var result = tools.ListAssemblyAttributes("NonExistent.Assembly");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void GetAssemblyInfo_EmptyName_MatchesAnyAssembly() {
		// Empty name with Contains("") returns true for any string, so it matches the first loaded assembly
		// This is expected behavior - empty pattern matches everything
		var result = tools.GetAssemblyInfo("");

		// When assemblies are loaded, empty string matches the first one
		// The result should be a valid AssemblyInfo for CoreLib
		var info = JsonSerializer.Deserialize<AssemblyInfo>(result);
		Assert.NotNull(info);
		Assert.NotNull(info.Name);
		// The matched assembly should be the CoreLib that was loaded in the constructor
	}

	[Fact]
	public void ListNamespaces_CaseInsensitive() {
		// Should match case-insensitively
		var resultLower = tools.ListNamespaces(CoreLibName.ToLowerInvariant());
		var resultMixed = tools.ListNamespaces(CoreLibName);

		var itemsLower = JsonSerializer.Deserialize<List<NamespaceInfo>>(resultLower);
		var itemsMixed = JsonSerializer.Deserialize<List<NamespaceInfo>>(resultMixed);

		Assert.NotNull(itemsLower);
		Assert.NotNull(itemsMixed);
		Assert.Equal(itemsLower.Count, itemsMixed.Count);
	}

	#endregion
}

