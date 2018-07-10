using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Cone.Core
{
#if NET45
    static class AssemblyLoader
    {
		public static Assembly LoadFrom(string path) {
			Console.WriteLine($"Loading {path}");
			return Assembly.LoadFrom(path);
		}

		public static void InitDeps(string mainAssembly) { }
    }
#else
	using System.Runtime.Loader;
	using Microsoft.DotNet.PlatformAbstractions;

	public struct DependencyItem
	{
		public readonly string Path;
		public readonly bool IsManaged;

		public DependencyItem(string path, bool isManaged) {
			this.Path = path;
			this.IsManaged = isManaged;
		}
	}

	public static class AssemblyLoader
	{		
		static ConeLoadContext LoadContext;

		class ConeLoadContext : AssemblyLoadContext
		{
			public readonly string BasePath;
			public readonly AssemblyJsonDeps Deps;
			public readonly RuntimeInfo Runtime;

			public ConeLoadContext(string basePath, AssemblyJsonDeps deps, RuntimeInfo runtime) {
				this.BasePath = basePath;
				this.Deps = deps;
				this.Runtime = runtime;
			}

			public Assembly OnResolving(AssemblyLoadContext _, AssemblyName e) {
				WriteDiagnostic($"Resolving {e.Name}");
				foreach (var item in LoadContext.Resolve(e.Name)) {
					WriteDiagnostic($"  Loading Dependency {item.Path} for {e.Name}");
					Load(item);
				}
				if (Deps.TryGetTarget(e.Name, out var found)) {
					WriteDiagnostic($"  Found {e.Name}");

					var xs = found.runtime.Select(x => LoadFromAssemblyPath(Path.Combine(BasePath, x.Key))).ToList();
					return xs.First(x => x.GetName() == e);
				}
				return null;
			}

			public void Load(DependencyItem dep) {
				if(dep.IsManaged)
					Default.LoadFromAssemblyPath(dep.Path);
				else LoadUnmanagedDllFromPath(dep.Path);
			}

			public IEnumerable<DependencyItem> Resolve(string depName) { 
				var found = new List<DependencyItem>();
				Resolve(depName, new HashSet<string>(), found.Add);
				return found;
			}

			void Resolve(string depName, HashSet<string> seen, Action<DependencyItem> onFound) {
				if (!seen.Add(depName) || !Deps.TryGetLibraryDep(depName, out var found))
					return;

				string foundDep = null;
				var depIsManaged = true;

				foreach (var probePath in Runtime.PackageProbePaths) {
					var managedPath = $@"{probePath}\{found.path}\runtimes\{Runtime.OS}\lib\{Runtime.FrameworkVersion}";
					if (Directory.Exists(managedPath)) {
						foundDep = managedPath;
						depIsManaged = true;
						break;
					}
					else {
						var nativepath = $@"{probePath}\{found.path}\runtimes\{Runtime.OS}-{Runtime.Arch}\native";
						if (Directory.Exists(nativepath)) {
							foundDep = nativepath;
							depIsManaged = false;
							break;
						}
					}
				}

				if (foundDep != null)
					foreach (var item in Directory.GetFiles(foundDep))
						onFound(new DependencyItem(item, depIsManaged));

				foreach (var child in Deps.DepsFor(found.FullKey))
					Resolve(child.Split('/')[0], seen, onFound);
			}


			protected override Assembly Load(AssemblyName assemblyName) => Default.LoadFromAssemblyName(assemblyName);
		}

		public static Assembly LoadFrom(string path) {
			if (LoadContext == null)
				InitDeps(path);
			return LoadContext.LoadFromAssemblyPath(path);
		}

		public static void InitDeps(string mainAssembly) {
			if (LoadContext != null) {
				AssemblyLoadContext.Default.Resolving -= LoadContext.OnResolving;
			}
			LoadContext = new ConeLoadContext(Path.GetDirectoryName(mainAssembly), AssemblyJsonDeps.LoadFrom(mainAssembly), new RuntimeInfo {
				OS = Environment.GetEnvironmentVariable("CONE_RUNTIME_OS") ?? (string)typeof(RuntimeEnvironment).GetMethod("GetRIDOS", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null),
				Arch = RuntimeEnvironment.RuntimeArchitecture,
				//FrameworkVersion = "netstandard2.0",
				FrameworkVersion = Environment.GetEnvironmentVariable("CONE_TARGET_FRAMEWORK"),
				PackageProbePaths = AppContext.GetData("PROBING_DIRECTORIES").ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
			});

			AssemblyLoadContext.Default.Resolving += LoadContext.OnResolving;
		}

		static void WriteDiagnostic(string message) {
			return;
			var fc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.WriteLine(message);
			Console.ForegroundColor = fc;
		}
	}
#endif

	public class AssemblyJsonDeps
	{
		const string DepsSuffix = "deps.json";

		public class JsonDeps
		{
			public JsonDepsRuntimeTarget runtimeTarget;
			public Dictionary<string, Dictionary<string, JsonTargetDep>> targets;
			public Dictionary<string, JsonLibraryDep> libraries;

			public Dictionary<string, JsonTargetDep> CurrentRuntimeDeps => targets[runtimeTarget.name];
		}

		public class JsonDepsRuntimeTarget
		{
			public string name;
		}

		public class JsonTargetDep
		{
			public string FullKey;
			public Dictionary<string, string> dependencies;
			public Dictionary<string, JsonRuntimeVersionInfo> runtime;
			public Dictionary<string, JsonRuntimeTarget> runtimeTargets;
		}

		public class JsonRuntimeTarget
		{
			public string rid;
		}

		public struct JsonRuntimeVersionInfo
		{
			public bool IsEmpty => string.IsNullOrEmpty(assemblyVersion) && string.IsNullOrEmpty(fileVersion);
			public string assemblyVersion;
			public string fileVersion;
		}

		public class JsonLibraryDep
		{
			public string FullKey;
			public string type;
			public bool servicable;
			public string sha512;
			public string path;
			public string hashPath;
		}

		public JsonDeps deps;
		readonly Dictionary<string, JsonTargetDep> targetLookup = new Dictionary<string, JsonTargetDep>();
		readonly Dictionary<string, JsonLibraryDep> libraryLookup = new Dictionary<string, JsonLibraryDep>();

		public bool TryGetLibraryDep(string key, out JsonLibraryDep found) => libraryLookup.TryGetValue(key, out found);

		public bool TryGetTarget(string name, out JsonTargetDep found) => targetLookup.TryGetValue(name, out found);

		public IEnumerable<string> DepsFor(string name) { 
			if(deps.CurrentRuntimeDeps.TryGetValue(name, out var found) && found.dependencies != null) { 
				return found.dependencies.Select(x => x.Key);
			} else 
				return Enumerable.Empty<string>();
		}

		public static AssemblyJsonDeps LoadFrom(string assemblyPath) {
			var depsPath = Path.ChangeExtension(assemblyPath, DepsSuffix);
			var result = new AssemblyJsonDeps();
			if (File.Exists(depsPath)) {
				result.deps = JsonConvert.DeserializeObject<JsonDeps>(File.ReadAllText(depsPath));

				foreach(var item in result.deps.targets[result.deps.runtimeTarget.name]) { 
					item.Value.FullKey = item.Key;
					result.targetLookup.Add(NameOnly(item.Key), item.Value);
				}
				foreach (var item in result.deps.libraries) {
					item.Value.FullKey = item.Key;
					result.libraryLookup.Add(NameOnly(item.Key), item.Value);
				}
			}
			return result;
		}

		static string NameOnly(string nameVersion) => nameVersion.Substring(0, nameVersion.IndexOf('/'));

		public override string ToString() => JsonConvert.SerializeObject(deps, Formatting.Indented);
	}

	public class RuntimeInfo
	{
		public string OS;
		public string Arch;
		public string FrameworkVersion;
		public string[] PackageProbePaths;
	}

}
