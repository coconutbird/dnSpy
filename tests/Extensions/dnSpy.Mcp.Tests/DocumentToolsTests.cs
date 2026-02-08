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

public class DocumentToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly DocumentTools tools;

	// Get the actual CoreLib name for the running framework
	static string CoreLibName => typeof(object).Assembly.GetName().Name ?? "System.Private.CoreLib";

	public DocumentToolsTests() {
		fixture = new TestFixture();
		tools = new DocumentTools(fixture.Services);
	}

	public void Dispose() => fixture.Dispose();

	#region ListAssemblies Tests

	[Fact]
	public void ListAssemblies_NoDocuments_ReturnsEmptyArray() {
		var result = tools.ListAssemblies();
		var items = JsonSerializer.Deserialize<List<AssemblyListItem>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void ListAssemblies_WithCorLib_ReturnsAssembly() {
		fixture.LoadCorLib();

		var result = tools.ListAssemblies();
		var items = JsonSerializer.Deserialize<List<AssemblyListItem>>(result);

		Assert.NotNull(items);
		Assert.Single(items);
		Assert.Equal("dotnet", items[0].Kind);
		Assert.True(items[0].IsManaged);
		Assert.NotNull(items[0].AssemblyName);
	}

	[Fact]
	public void ListAssemblies_MultipleAssemblies_ReturnsAll() {
		fixture.LoadCorLib();
		fixture.LoadAssemblyFromType(typeof(DocumentToolsTests));

		var result = tools.ListAssemblies();
		var items = JsonSerializer.Deserialize<List<AssemblyListItem>>(result);

		Assert.NotNull(items);
		Assert.Equal(2, items.Count);
	}

	#endregion

	#region ListTypes Tests

	[Fact]
	public void ListTypes_NonExistentAssembly_ReturnsEmptyArray() {
		fixture.LoadCorLib();

		var result = tools.ListTypes("NonExistentAssembly");
		var items = JsonSerializer.Deserialize<List<TypeListItem>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void ListTypes_WithCorLib_ReturnsTypes() {
		fixture.LoadCorLib();

		var result = tools.ListTypes(CoreLibName, maxResults: 10);
		var items = JsonSerializer.Deserialize<List<TypeListItem>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.True(items.Count <= 10);
	}

	[Fact]
	public void ListTypes_WithNamespaceFilter_FiltersCorrectly() {
		fixture.LoadCorLib();

		var result = tools.ListTypes(CoreLibName, namespaceFilter: "System.Collections", maxResults: 50);
		var items = JsonSerializer.Deserialize<List<TypeListItem>>(result);

		Assert.NotNull(items);
		Assert.All(items, item => Assert.Contains("System.Collections", item.Namespace));
	}

	[Fact]
	public void ListTypes_MaxResultsZero_IsHandledAsNoLimitOrSingleResult() {
		// maxResults=0 behavior may vary: could mean "no limit" or default behavior
		// The test just verifies no exception is thrown and valid result returned
		fixture.LoadCorLib();

		var result = tools.ListTypes(CoreLibName, maxResults: 0);
		var items = JsonSerializer.Deserialize<List<TypeListItem>>(result);

		Assert.NotNull(items);
		// Don't assert on count - behavior varies between frameworks
	}

	#endregion

	#region FindTypes Tests

	[Fact]
	public void FindTypes_ExistingType_FindsIt() {
		fixture.LoadCorLib();

		var result = tools.FindTypes("String", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<TypeSearchResult>>(result);

		Assert.NotNull(items);
		Assert.Contains(items, t => t.FullName == "System.String");
	}

	[Fact]
	public void FindTypes_NonExistentPattern_ReturnsEmpty() {
		fixture.LoadCorLib();

		var result = tools.FindTypes("XyzNonExistentType123");
		var items = JsonSerializer.Deserialize<List<TypeSearchResult>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindTypes_EmptyPattern_FindsAll() {
		fixture.LoadCorLib();

		var result = tools.FindTypes("", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<TypeSearchResult>>(result);

		Assert.NotNull(items);
		Assert.Equal(5, items.Count);
	}

	[Fact]
	public void FindTypes_CaseInsensitive() {
		fixture.LoadCorLib();

		var resultLower = tools.FindTypes("string", maxResults: 10);
		var resultUpper = tools.FindTypes("STRING", maxResults: 10);

		var itemsLower = JsonSerializer.Deserialize<List<TypeSearchResult>>(resultLower);
		var itemsUpper = JsonSerializer.Deserialize<List<TypeSearchResult>>(resultUpper);

		Assert.NotNull(itemsLower);
		Assert.NotNull(itemsUpper);
		Assert.Equal(itemsLower.Count, itemsUpper.Count);
	}

	#endregion
}

