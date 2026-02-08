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

public class ILToolsTests : IDisposable {
	readonly TestFixture fixture;
	readonly ILTools tools;

	public ILToolsTests() {
		fixture = new TestFixture();
		tools = new ILTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	#region GetILInstructions Tests

	[Fact]
	public void GetILInstructions_ValidMethod_ReturnsInstructions() {
		var result = tools.GetILInstructions("System.String", "ToString");
		var items = JsonSerializer.Deserialize<List<ILInstruction>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count > 0);
		Assert.All(items, i => Assert.NotNull(i.OpCode));
	}

	[Fact]
	public void GetILInstructions_NonExistentMethod_ReturnsError() {
		var result = tools.GetILInstructions("System.String", "NonExistentMethod");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetILInstructions_NonExistentType_ReturnsError() {
		var result = tools.GetILInstructions("NonExistent.Type", "SomeMethod");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetILInstructions_MaxInstructionsRespected() {
		var result = tools.GetILInstructions("System.String", "Concat", maxInstructions: 5);
		var items = JsonSerializer.Deserialize<List<ILInstruction>>(result);

		Assert.NotNull(items);
		Assert.True(items.Count <= 5);
	}

	#endregion

	#region GetMethodBody Tests

	[Fact]
	public void GetMethodBody_ValidMethod_ReturnsInfo() {
		var result = tools.GetMethodBody("System.String", "ToString");
		var info = JsonSerializer.Deserialize<MethodBodyInfo>(result);

		Assert.NotNull(info);
		Assert.True(info.CodeSize > 0);
	}

	[Fact]
	public void GetMethodBody_NonExistentMethod_ReturnsError() {
		var result = tools.GetMethodBody("System.String", "NonExistentMethod");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetMethodBody_AbstractMethod_ReturnsError() {
		// Abstract methods have no body
		var result = tools.GetMethodBody("System.IDisposable", "Dispose");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("no body", error.Error);
	}

	#endregion

	#region GetMethodBytes Tests

	[Fact]
	public void GetMethodBytes_ValidMethod_ReturnsBytes() {
		var result = tools.GetMethodBytes("System.String", "ToString");
		var info = JsonSerializer.Deserialize<MethodBytesInfo>(result);

		Assert.NotNull(info);
		Assert.NotNull(info.Bytes);
		Assert.True(info.Bytes.Length > 0);
	}

	[Fact]
	public void GetMethodBytes_NonExistentMethod_ReturnsError() {
		var result = tools.GetMethodBytes("System.String", "NonExistentMethod");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void GetILInstructions_EmptyTypeName_ReturnsError() {
		var result = tools.GetILInstructions("", "SomeMethod");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetILInstructions_EmptyMethodName_ReturnsError() {
		var result = tools.GetILInstructions("System.String", "");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	#endregion
}

