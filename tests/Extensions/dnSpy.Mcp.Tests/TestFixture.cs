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

using dnlib.DotNet;
using dnlib.PE;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Mcp.Tools;
using Moq;

namespace dnSpy.Mcp.Tests;

/// <summary>
/// Mock implementation of IDsDocument that wraps a real dnlib ModuleDef
/// </summary>
public class MockDsDocument : IDsDocument {
	readonly ModuleDef moduleDef;
	readonly string filename;
	readonly List<object> annotations = new();

	public MockDsDocument(ModuleDef moduleDef, string filename) {
		this.moduleDef = moduleDef;
		this.filename = filename;
	}

	public DsDocumentInfo? SerializedDocument => null;
	public IDsDocumentNameKey Key => new FilenameKey(filename);
	public AssemblyDef? AssemblyDef => moduleDef.Assembly;
	public ModuleDef? ModuleDef => moduleDef;
	public IPEImage? PEImage => (moduleDef as ModuleDefMD)?.Metadata?.PEImage;
	public string Filename { get => filename; set { } }
	public bool IsAutoLoaded { get; set; }
	public TList<IDsDocument> Children { get; } = new TList<IDsDocument>();
	public bool ChildrenLoaded => true;

	// IAnnotations implementation
	public T? AddAnnotation<T>(T? annotation) where T : class {
		if (annotation is not null)
			annotations.Add(annotation);
		return annotation;
	}

	public T? Annotation<T>() where T : class => (T?)annotations.FirstOrDefault(a => a is T);

	public IEnumerable<T> Annotations<T>() where T : class => annotations.OfType<T>();

	public void RemoveAnnotations<T>() where T : class {
		for (int i = annotations.Count - 1; i >= 0; i--) {
			if (annotations[i] is T)
				annotations.RemoveAt(i);
		}
	}
}

/// <summary>
/// Test fixture that provides mocked dnSpy services for functional testing
/// </summary>
public class TestFixture : IDisposable {
	readonly List<ModuleDef> loadedModules = new();
	readonly Mock<IDsDocumentService> mockDocumentService;
	readonly Mock<IDocumentTabService> mockDocumentTabService;
	readonly Mock<IDecompilerService> mockDecompilerService;
	readonly Mock<IAppWindow> mockAppWindow;
	readonly List<IDsDocument> documents = new();

	public DnSpyServices Services { get; }
	public IReadOnlyList<ModuleDef> LoadedModules => loadedModules;

	public TestFixture() {
		mockDocumentService = new Mock<IDsDocumentService>();
		mockDocumentService.Setup(x => x.GetDocuments()).Returns(() => documents.ToArray());

		mockDocumentTabService = new Mock<IDocumentTabService>();
		mockDecompilerService = new Mock<IDecompilerService>();
		mockAppWindow = new Mock<IAppWindow>();

		Services = new DnSpyServices(
			mockDocumentService.Object,
			mockDocumentTabService.Object,
			mockDecompilerService.Object,
			mockAppWindow.Object);
	}

	/// <summary>
	/// Load a .NET assembly from a file path
	/// </summary>
	public ModuleDef LoadAssembly(string filePath) {
		var moduleDef = ModuleDefMD.Load(filePath);
		loadedModules.Add(moduleDef);
		documents.Add(new MockDsDocument(moduleDef, filePath));
		return moduleDef;
	}

	/// <summary>
	/// Load a .NET assembly from a Type (uses the assembly containing the type)
	/// </summary>
	public ModuleDef LoadAssemblyFromType(Type type) {
		var assemblyPath = type.Assembly.Location;
		return LoadAssembly(assemblyPath);
	}

	/// <summary>
	/// Load mscorlib/System.Private.CoreLib
	/// </summary>
	public ModuleDef LoadCorLib() {
		var corLibPath = typeof(object).Assembly.Location;
		return LoadAssembly(corLibPath);
	}

	/// <summary>
	/// Clear all loaded documents
	/// </summary>
	public void ClearDocuments() {
		documents.Clear();
	}

	public void Dispose() {
		foreach (var module in loadedModules) {
			module.Dispose();
		}
		loadedModules.Clear();
		documents.Clear();
	}
}

