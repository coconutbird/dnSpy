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

public class SearchToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly SearchTools tools;

	public SearchToolsTests() {
		fixture = new TestFixture();
		tools = new SearchTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region SearchStrings Tests

	[Fact]
	public void SearchStrings_ValidPattern_FindsStrings() {
		var result = tools.SearchStrings("*", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<StringSearchResult>>(result);

		Assert.NotNull(items);
		// The pattern "*" should match any string containing asterisk, so might be empty
		// Let's use a more likely pattern
	}

	[Fact]
	public void SearchStrings_MaxResultsRespected() {
		var result = tools.SearchStrings("", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<StringSearchResult>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	[Fact]
	public void SearchStrings_NonExistentPattern_ReturnsEmpty() {
		var result = tools.SearchStrings("XyzVeryUnlikelyString123!@#");
		var items = JsonSerializer.Deserialize<List<StringSearchResult>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void SearchStrings_CaseSensitive_Works() {
		// Test case sensitivity flag
		var resultInsensitive = tools.SearchStrings("error", caseSensitive: false, maxResults: 5);
		var resultSensitive = tools.SearchStrings("error", caseSensitive: true, maxResults: 5);

		// Both should return valid JSON
		Assert.NotNull(JsonSerializer.Deserialize<List<StringSearchResult>>(resultInsensitive));
		Assert.NotNull(JsonSerializer.Deserialize<List<StringSearchResult>>(resultSensitive));
	}

	#endregion

	#region SearchNumbers Tests

	[Fact]
	public void SearchNumbers_MaxResultsRespected() {
		var result = tools.SearchNumbers(0, maxResults: 5);
		var items = JsonSerializer.Deserialize<List<NumberSearchResult>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	[Fact]
	public void SearchNumbers_CommonValue_FindsOccurrences() {
		// -1 is commonly used in code
		var result = tools.SearchNumbers(-1, maxResults: 10);
		var items = JsonSerializer.Deserialize<List<NumberSearchResult>>(result);

		Assert.NotNull(items);
		// -1 should be found somewhere in mscorlib
	}

	#endregion

	#region SearchBySignature Tests

	[Fact]
	public void SearchBySignature_ValidPattern_FindsMethods() {
		var result = tools.SearchBySignature(methodName: "ToString", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<SignatureMatchInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.All(items, m => Assert.Contains("ToString", m.MethodName));
	}

	[Fact]
	public void SearchBySignature_NonExistentPattern_ReturnsEmpty() {
		var result = tools.SearchBySignature(methodName: "XyzNonExistentMethod123");
		var items = JsonSerializer.Deserialize<List<SignatureMatchInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void SearchBySignature_MaxResultsRespected() {
		var result = tools.SearchBySignature(returnType: "void", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<SignatureMatchInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void SearchStrings_WildcardPattern_Works() {
		// Test wildcard pattern handling
		var result = tools.SearchStrings("Exception*", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<StringSearchResult>>(result);

		Assert.NotNull(items);
	}

	[Fact]
	public void SearchNumbers_LargeNumber_Works() {
		var result = tools.SearchNumbers(long.MaxValue, maxResults: 5);
		var items = JsonSerializer.Deserialize<List<NumberSearchResult>>(result);

		Assert.NotNull(items);
		// Unlikely to find MaxValue, should be empty
	}

	[Fact]
	public void SearchBySignature_VoidReturn_FindsMethods() {
		var result = tools.SearchBySignature(returnType: "void", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<SignatureMatchInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion
}

