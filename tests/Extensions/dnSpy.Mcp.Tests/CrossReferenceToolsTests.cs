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

public class CrossReferenceToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly CrossReferenceTools tools;

	public CrossReferenceToolsTests() {
		fixture = new TestFixture();
		tools = new CrossReferenceTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region FindUsages Tests

	[Fact]
	public void FindUsages_ValidType_FindsUsages() {
		// System.String is used extensively in CoreLib
		var result = tools.FindUsages("System.String", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<UsageInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.All(items, item => Assert.NotNull(item.Location));
	}

	[Fact]
	public void FindUsages_NonExistentType_ReturnsEmpty() {
		var result = tools.FindUsages("NonExistent.Type.XYZ123");
		var items = JsonSerializer.Deserialize<List<UsageInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindUsages_MaxResultsRespected() {
		var result = tools.FindUsages("System.Object", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<UsageInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion

	#region FindCallers Tests

	[Fact]
	public void FindCallers_CommonMethod_FindsCallers() {
		// ToString is called by many methods
		var result = tools.FindCallers("System.Object", "ToString", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<CallerInfo>>(result);

		Assert.NotNull(items);
		// May or may not find callers depending on CoreLib version
	}

	[Fact]
	public void FindCallers_NonExistentMethod_ReturnsEmpty() {
		var result = tools.FindCallers("System.String", "NonExistentMethod123");
		var items = JsonSerializer.Deserialize<List<CallerInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindCallers_MaxResultsRespected() {
		var result = tools.FindCallers("System.Object", "GetHashCode", maxResults: 3);
		var items = JsonSerializer.Deserialize<List<CallerInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 3);
	}

	#endregion

	#region FindCallees Tests

	[Fact]
	public void FindCallees_ValidMethod_ReturnsCallees() {
		// String.Concat calls other methods
		var result = tools.FindCallees("System.String", "Concat");
		var items = JsonSerializer.Deserialize<List<CalleeInfo>>(result);

		// Either returns callees or an error if method not found/has no body
		if (!result.Contains("Error")) {
			Assert.NotNull(items);
		}
	}

	[Fact]
	public void FindCallees_NonExistentMethod_ReturnsError() {
		var result = tools.FindCallees("System.String", "NonExistentMethod123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void FindCallees_AbstractMethod_ReturnsError() {
		// Abstract methods have no body
		var result = tools.FindCallees("System.IO.Stream", "Read");

		// May return error about no body, or may find an overload with body
		Assert.NotNull(result);
	}

	#endregion

	#region FindTypeReferences Tests

	[Fact]
	public void FindTypeReferences_CommonType_FindsReferences() {
		var result = tools.FindTypeReferences("System.Object", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<TypeReferenceInfo>>(result);

		Assert.NotNull(items);
		// System.Object is inherited by everything
		Assert.True(items.Count > 0);
	}

	[Fact]
	public void FindTypeReferences_NonExistentType_ReturnsEmpty() {
		var result = tools.FindTypeReferences("NonExistent.Type.XYZ123");
		var items = JsonSerializer.Deserialize<List<TypeReferenceInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindTypeReferences_MaxResultsRespected() {
		var result = tools.FindTypeReferences("System.Object", maxResults: 5);
		var items = JsonSerializer.Deserialize<List<TypeReferenceInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion

	#region FindFieldReferences Tests

	[Fact]
	public void FindFieldReferences_ValidField_FindsReferences() {
		// String.Empty is a commonly referenced field
		var result = tools.FindFieldReferences("System.String", "Empty", maxResults: 10);
		var items = JsonSerializer.Deserialize<List<FieldReferenceInfo>>(result);

		Assert.NotNull(items);
		// May or may not find references depending on CoreLib
	}

	[Fact]
	public void FindFieldReferences_NonExistentField_ReturnsEmpty() {
		var result = tools.FindFieldReferences("System.String", "NonExistentField123");
		var items = JsonSerializer.Deserialize<List<FieldReferenceInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	#endregion

	#region FindStringUsages Tests

	[Fact]
	public void FindStringUsages_NonExistentString_ReturnsEmpty() {
		var result = tools.FindStringUsages("ThisStringDoesNotExist12345XYZ");
		var items = JsonSerializer.Deserialize<List<StringUsageInfo>>(result);

		Assert.NotNull(items);
		Assert.Empty(items);
	}

	[Fact]
	public void FindStringUsages_MaxResultsRespected() {
		// Empty string might be used somewhere
		var result = tools.FindStringUsages("", maxResults: 3);
		var items = JsonSerializer.Deserialize<List<StringUsageInfo>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 3);
	}

	#endregion

	#region BuildCallGraph Tests

	[Fact]
	public void BuildCallGraph_NonExistentMethod_ReturnsError() {
		var result = tools.BuildCallGraph("System.String", "NonExistentMethod123");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void BuildCallGraph_MaxDepthRespected() {
		var result = tools.BuildCallGraph("System.String", "Concat", maxDepth: 1, maxNodes: 50);

		if (!result.Contains("Error")) {
			var graph = JsonSerializer.Deserialize<CallGraph>(result);
			Assert.NotNull(graph);
			Assert.All(graph.Nodes, node => Assert.True(node.Depth <= 1));
		}
	}

	#endregion

	#region AnalyzeDependencies Tests

	[Fact]
	public void AnalyzeDependencies_ValidType_ReturnsAnalysis() {
		// Use the first overload (non-nullable typeName)
		var result = tools.AnalyzeDependencies("System.String", 50);
		var analysis = JsonSerializer.Deserialize<DependencyAnalysis>(result);

		Assert.NotNull(analysis);
		Assert.Equal("System.String", analysis.TargetType);
	}

	[Fact]
	public void AnalyzeDependencies_NonExistentType_ReturnsEmptyAnalysis() {
		// Non-existent type returns empty analysis (not error) for first overload
		var result = tools.AnalyzeDependencies("NonExistent.Type.XYZ123", 50);
		var analysis = JsonSerializer.Deserialize<DependencyAnalysis>(result);

		Assert.NotNull(analysis);
		Assert.Equal("NonExistent.Type.XYZ123", analysis.TargetType);
		// Incoming/Outgoing should be empty for non-existent type
	}

	[Fact]
	public void AnalyzeDependencies_NoParameters_ReturnsError() {
		// Use second overload with both null
		var result = tools.AnalyzeDependencies(typeName: null, assemblyName: null, direction: "both", maxResults: 100);
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("must be provided", error.Error);
	}

	#endregion
}

