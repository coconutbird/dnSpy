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

public class MemberToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly MemberTools tools;

	public MemberToolsTests() {
		fixture = new TestFixture();
		tools = new MemberTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region FindFields Tests

	[Fact]
	public void FindFields_ValidPattern_FindsFields() {
		var result = tools.FindFields("Empty", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<FieldSearchResult>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.Contains(items, f => f.FieldName.Contains("Empty"));
	}

	[Fact]
	public void FindFields_NonExistentPattern_ReturnsEmpty() {
		var result = tools.FindFields("XyzNonExistent123", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<FieldSearchResult>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindFields_MaxResultsRespected() {
		var result = tools.FindFields("", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<FieldSearchResult>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion

	#region ListFields Tests

	[Fact]
	public void ListFields_ValidType_ReturnsFields() {
		var result = tools.ListFields("System.String");
		var items = JsonSerializer.Deserialize<List<FieldListItem>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
	}

	[Fact]
	public void ListFields_NonExistentType_ReturnsError() {
		var result = tools.ListFields("NonExistent.Type");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void ListFields_EmptyTypeName_ReturnsError() {
		var result = tools.ListFields("");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region GetFieldInfo Tests

	[Fact]
	public void GetFieldInfo_ValidField_ReturnsInfo() {
		var result = tools.GetFieldInfo("System.String", "Empty");
		var info = JsonSerializer.Deserialize<FieldInfo>(result);

		Assert.NotNull(info);
		Assert.Equal("Empty", info.Name);
		Assert.True(info.IsStatic);
		Assert.True(info.IsPublic);
	}

	[Fact]
	public void GetFieldInfo_NonExistentField_ReturnsError() {
		var result = tools.GetFieldInfo("System.String", "NonExistentField");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetFieldInfo_NonExistentType_ReturnsError() {
		var result = tools.GetFieldInfo("NonExistent.Type", "SomeField");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region FindProperties Tests

	[Fact]
	public void FindProperties_ValidPattern_FindsProperties() {
		var result = tools.FindProperties("Length", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<PropertySearchResult>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.Contains(items, p => p.PropertyName == "Length");
	}

	[Fact]
	public void FindProperties_NonExistentPattern_ReturnsEmpty() {
		var result = tools.FindProperties("XyzNonExistent123", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<PropertySearchResult>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	#endregion
}

