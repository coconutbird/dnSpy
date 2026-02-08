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

public class ResourceToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly ResourceTools tools;

	// Get the actual CoreLib name for the running framework
	static string CoreLibName => typeof(object).Assembly.GetName().Name ?? "System.Private.CoreLib";

	public ResourceToolsTests() {
		fixture = new TestFixture();
		tools = new ResourceTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region ListResources Tests

	[Fact]
	public void ListResources_ValidAssembly_ReturnsList() {
		var result = tools.ListResources(CoreLibName);
		var items = JsonSerializer.Deserialize<List<ResourceListItem>>(result);

		Assert.NotNull(items);
		// CoreLib may or may not have resources depending on version
	}

	[Fact]
	public void ListResources_NonExistentAssembly_ReturnsError() {
		var result = tools.ListResources("NonExistent.Assembly.XYZ123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void ListResources_EmptyName_MatchesAnyAssembly() {
		// Empty name matches any assembly (same behavior as AssemblyTools)
		var result = tools.ListResources("");
		var items = JsonSerializer.Deserialize<List<ResourceListItem>>(result);

		Assert.NotNull(items);
		// Should return resources from the first matched assembly
	}

	#endregion

	#region ListResourceSets Tests

	[Fact]
	public void ListResourceSets_ValidAssembly_ReturnsList() {
		var result = tools.ListResourceSets(CoreLibName);
		var items = JsonSerializer.Deserialize<List<ResourceSetInfo>>(result);

		Assert.NotNull(items);
		// CoreLib may have .resources files
	}

	[Fact]
	public void ListResourceSets_NonExistentAssembly_ReturnsError() {
		var result = tools.ListResourceSets("NonExistent.Assembly.XYZ123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region GetResource Tests

	[Fact]
	public void GetResource_NonExistentAssembly_ReturnsError() {
		var result = tools.GetResource("NonExistent.Assembly", "SomeResource");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetResource_NonExistentResource_ReturnsError() {
		var result = tools.GetResource(CoreLibName, "NonExistent.Resource.XYZ123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region GetResourceInfo Tests

	[Fact]
	public void GetResourceInfo_NonExistentAssembly_ReturnsError() {
		var result = tools.GetResourceInfo("NonExistent.Assembly", "SomeResource");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetResourceInfo_NonExistentResource_ReturnsError() {
		var result = tools.GetResourceInfo(CoreLibName, "NonExistent.Resource.XYZ123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region ListResourceEntries Tests

	[Fact]
	public void ListResourceEntries_NonExistentAssembly_ReturnsError() {
		var result = tools.ListResourceEntries("NonExistent.Assembly", "SomeResource.resources");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void ListResourceEntries_NonExistentResourceSet_ReturnsError() {
		var result = tools.ListResourceEntries(CoreLibName, "NonExistent.resources");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion
}

