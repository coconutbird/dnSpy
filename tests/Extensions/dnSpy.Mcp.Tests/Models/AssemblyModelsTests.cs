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
using Xunit;

namespace dnSpy.Mcp.Tests.Models;

public class AssemblyModelsTests {
	#region AssemblyInfo Tests

	[Fact]
	public void AssemblyInfo_DefaultValues_AreCorrect() {
		var info = new AssemblyInfo();

		Assert.Equal("", info.Name);
		Assert.Equal("", info.Version);
		Assert.Equal("", info.Culture);
		Assert.Equal("", info.PublicKeyToken);
		Assert.Equal("", info.FullName);
		Assert.Equal("", info.TargetFramework);
		Assert.Equal("", info.Architecture);
		Assert.Null(info.EntryPoint);
		Assert.False(info.IsDebug);
		Assert.False(info.IsSigned);
		Assert.Equal("", info.FilePath);
		Assert.Equal(0, info.CustomAttributeCount);
		Assert.NotNull(info.Modules);
		Assert.Empty(info.Modules);
	}

	[Fact]
	public void AssemblyInfo_Serialization_RoundTrip() {
		var original = new AssemblyInfo {
			Name = "TestAssembly",
			Version = "1.0.0.0",
			Culture = "neutral",
			PublicKeyToken = "b77a5c561934e089",
			FullName = "TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
			TargetFramework = ".NETCoreApp,Version=v8.0",
			Architecture = "x64",
			EntryPoint = "TestAssembly.Program.Main",
			IsDebug = true,
			IsSigned = true,
			FilePath = @"C:\Test\TestAssembly.dll",
			CustomAttributeCount = 5,
			Modules = new List<ModuleInfo> {
				new ModuleInfo { Name = "TestAssembly.dll", IsManifestModule = true, TypeCount = 10 }
			}
		};

		var json = JsonSerializer.Serialize(original);
		var deserialized = JsonSerializer.Deserialize<AssemblyInfo>(json);

		Assert.NotNull(deserialized);
		Assert.Equal(original.Name, deserialized.Name);
		Assert.Equal(original.Version, deserialized.Version);
		Assert.Equal(original.EntryPoint, deserialized.EntryPoint);
		Assert.Equal(original.IsDebug, deserialized.IsDebug);
		Assert.Single(deserialized.Modules);
	}

	#endregion

	#region ModuleInfo Tests

	[Fact]
	public void ModuleInfo_DefaultValues_AreCorrect() {
		var info = new ModuleInfo();

		Assert.Null(info.Name);
		Assert.Null(info.Mvid);
		Assert.Equal(0, info.TypeCount);
		Assert.False(info.IsManifestModule);
		Assert.Null(info.RuntimeVersion);
		Assert.Null(info.Location);
	}

	[Fact]
	public void ModuleInfo_Serialization_RoundTrip() {
		var original = new ModuleInfo {
			Name = "TestModule.dll",
			Mvid = "12345678-1234-1234-1234-123456789012",
			TypeCount = 100,
			IsManifestModule = true,
			RuntimeVersion = "v4.0.30319",
			Location = @"C:\Test\TestModule.dll"
		};

		var json = JsonSerializer.Serialize(original);
		var deserialized = JsonSerializer.Deserialize<ModuleInfo>(json);

		Assert.NotNull(deserialized);
		Assert.Equal(original.Name, deserialized.Name);
		Assert.Equal(original.Mvid, deserialized.Mvid);
		Assert.Equal(original.TypeCount, deserialized.TypeCount);
		Assert.Equal(original.IsManifestModule, deserialized.IsManifestModule);
	}

	#endregion

	#region AssemblyReference Tests

	[Fact]
	public void AssemblyReference_DefaultValues_AreCorrect() {
		var reference = new AssemblyReference();

		Assert.Equal("", reference.Name);
		Assert.Equal("", reference.Version);
		Assert.Equal("", reference.Culture);
		Assert.Equal("", reference.PublicKeyToken);
		Assert.Equal("", reference.FullName);
	}

	[Fact]
	public void AssemblyReference_Serialization_RoundTrip() {
		var original = new AssemblyReference {
			Name = "System.Runtime",
			Version = "8.0.0.0",
			Culture = "neutral",
			PublicKeyToken = "b03f5f7f11d50a3a",
			FullName = "System.Runtime, Version=8.0.0.0"
		};

		var json = JsonSerializer.Serialize(original);
		var deserialized = JsonSerializer.Deserialize<AssemblyReference>(json);

		Assert.NotNull(deserialized);
		Assert.Equal(original.Name, deserialized.Name);
		Assert.Equal(original.Version, deserialized.Version);
	}

	#endregion
}

