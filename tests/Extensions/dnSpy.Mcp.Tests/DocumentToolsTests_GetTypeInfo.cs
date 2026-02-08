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

public class DocumentToolsTests_GetTypeInfo : IDisposable {
	readonly TestFixture fixture;
	readonly DocumentTools tools;

	public DocumentToolsTests_GetTypeInfo() {
		fixture = new TestFixture();
		tools = new DocumentTools(fixture.Services);
		fixture.LoadCorLib();
	}

	public void Dispose() => fixture.Dispose();

	[Fact]
	public void GetTypeInfo_SystemString_ReturnsCorrectInfo() {
		var result = tools.GetTypeInfo("System.String");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.Equal("System.String", info.FullName);
		Assert.Equal("System", info.Namespace);
		Assert.Equal("String", info.Name);
		Assert.Equal("class", info.Kind);
		Assert.True(info.IsSealed);
		Assert.True(info.IsPublic);
	}

	[Fact]
	public void GetTypeInfo_NonExistentType_ReturnsError() {
		var result = tools.GetTypeInfo("NonExistent.Type.Name");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetTypeInfo_Interface_ReturnsInterfaceKind() {
		var result = tools.GetTypeInfo("System.IDisposable");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.Equal("interface", info.Kind);
		Assert.True(info.IsAbstract);
	}

	[Fact]
	public void GetTypeInfo_Enum_ReturnsEnumKind() {
		var result = tools.GetTypeInfo("System.DayOfWeek");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.Equal("enum", info.Kind);
	}

	[Fact]
	public void GetTypeInfo_Struct_ReturnsStructKind() {
		var result = tools.GetTypeInfo("System.Int32");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.Equal("struct", info.Kind);
		Assert.True(info.IsSealed);
	}

	[Fact]
	public void GetTypeInfo_GenericType_HasGenericParameters() {
		var result = tools.GetTypeInfo("System.Collections.Generic.List`1");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.NotNull(info.GenericParameters);
		Assert.Single(info.GenericParameters);
	}

	[Fact]
	public void GetTypeInfo_TypeWithMethods_HasMethods() {
		var result = tools.GetTypeInfo("System.String");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.NotNull(info.Methods);
		Assert.True(info.Methods.Count > 0);
		Assert.Contains(info.Methods, m => m.Name == "ToString");
	}

	[Fact]
	public void GetTypeInfo_TypeWithInterfaces_ListsInterfaces() {
		var result = tools.GetTypeInfo("System.String");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.NotNull(info.Interfaces);
		Assert.True(info.Interfaces.Count > 0);
	}

	[Fact]
	public void GetTypeInfo_EmptyTypeName_ReturnsError() {
		var result = tools.GetTypeInfo("");
		var error = JsonSerializer.Deserialize<ErrorResponse>(result);

		Assert.NotNull(error);
		Assert.Contains("not found", error.Error);
	}

	[Fact]
	public void GetTypeInfo_AbstractClass_IsAbstract() {
		var result = tools.GetTypeInfo("System.Array");
		var info = JsonSerializer.Deserialize<TypeInfo>(result);

		Assert.NotNull(info);
		Assert.True(info.IsAbstract);
		Assert.Equal("class", info.Kind);
	}
}

