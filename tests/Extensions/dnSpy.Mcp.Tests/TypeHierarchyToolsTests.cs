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

public class TypeHierarchyToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly TypeHierarchyTools tools;

	public TypeHierarchyToolsTests() {
		fixture = new TestFixture();
		tools = new TypeHierarchyTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region GetTypeHierarchy Tests

	[Fact]
	public void GetTypeHierarchy_ValidType_ReturnsHierarchy() {
		var result = tools.GetTypeHierarchy("System.String");
		var hierarchy = JsonSerializer.Deserialize<TypeHierarchy>(result);

		Assert.NotNull(hierarchy);
		Assert.Equal("System.String", hierarchy.TypeName);
		Assert.Contains("System.Object", hierarchy.BaseTypes);
	}

	[Fact]
	public void GetTypeHierarchy_NonExistentType_ReturnsError() {
		var result = tools.GetTypeHierarchy("NonExistent.Type");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetTypeHierarchy_SystemObject_HasNoBaseTypes() {
		var result = tools.GetTypeHierarchy("System.Object");
		var hierarchy = JsonSerializer.Deserialize<TypeHierarchy>(result);

		Assert.NotNull(hierarchy);
		Assert.Equal("System.Object", hierarchy.TypeName);
		Assert.Empty(hierarchy.BaseTypes);
	}

	[Fact]
	public void GetTypeHierarchy_ValidType_ReturnsInterfaces() {
		var result = tools.GetTypeHierarchy("System.String");
		var hierarchy = JsonSerializer.Deserialize<TypeHierarchy>(result);

		Assert.NotNull(hierarchy);
		Assert.True(hierarchy.Interfaces.Count > 0 || hierarchy.AllInterfaces.Count > 0);
		// String implements IComparable, IEnumerable, etc.
	}

	[Fact]
	public void GetTypeHierarchy_SystemObject_HasNoInterfaces() {
		var result = tools.GetTypeHierarchy("System.Object");
		var hierarchy = JsonSerializer.Deserialize<TypeHierarchy>(result);

		Assert.NotNull(hierarchy);
		Assert.Empty(hierarchy.Interfaces);
	}

	#endregion

	#region FindDerivedTypes Tests

	[Fact]
	public void FindDerivedTypes_SystemException_FindsDerivedTypes() {
		var result = tools.FindDerivedTypes("System.Exception", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<DerivedTypeInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		// There should be many exception types
	}

	[Fact]
	public void FindDerivedTypes_SealedType_ReturnsEmpty() {
		var result = tools.FindDerivedTypes("System.String");
		var items = JsonSerializer.Deserialize<List<DerivedTypeInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindDerivedTypes_NonExistentType_ReturnsEmptyArray() {
		// Non-existent types return empty array (not an error) - valid behavior since there are no derived types
		var result = tools.FindDerivedTypes("NonExistent.Type");
		var items = JsonSerializer.Deserialize<List<DerivedTypeInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindDerivedTypes_MaxResultsRespected() {
		var result = tools.FindDerivedTypes("System.Exception", maxResults: 3);
		var items = JsonSerializer.Deserialize<List<DerivedTypeInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 3);
	}

	#endregion

	#region FindImplementations Tests

	[Fact]
	public void FindImplementations_IDisposable_FindsImplementations() {
		var result = tools.FindImplementations("System.IDisposable", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<ImplementationInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
	}

	[Fact]
	public void FindImplementations_MaxResultsRespected() {
		var result = tools.FindImplementations("System.IDisposable", maxResults: 3);
		var items = JsonSerializer.Deserialize<List<ImplementationInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 3);
	}

	[Fact]
	public void FindImplementations_WithMemberName_FindsMemberImplementations() {
		var result = tools.FindImplementations("System.IDisposable", memberName: "Dispose", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<ImplementationInfo>>(result);

		Assert.NotNull(items);
		// Should find Dispose implementations
		Assert.All(items, i => Assert.Equal("Method", i.MemberKind));
	}

	#endregion
}

