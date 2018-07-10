using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace Cone.Core
{


#if NET45
    static class AssemblyLoader
    {
		public static Assembly LoadFrom(string path) {
			Console.WriteLine($"Loading {path}");
			return Assembly.LoadFrom(path);
		}
    }
#else
	using System.Runtime.Loader;

	public struct DependencyPath
	{
		public string Path;
		public bool IsManaged;
	}

	public static class AssemblyLoader
	{ 
		public static IEnumerable<DependencyPath> Resolve(AssemblyJsonDeps deps, RuntimeDep runtime, string depName, HashSet<string> seen) {
			if(!seen.Add(depName))
				yield break;

			if(!deps.TryGetLibraryDep(depName, out var found))
				yield break;
			string foundDep = null;
			var depIsManaged = true;

			foreach(var probePath in runtime.PackageProbePaths) {
				var managedPath = $@"{probePath}\{found.path}\runtimes\{runtime.OS}\lib\{runtime.FrameworkVersion}";
				if (Directory.Exists(managedPath)) {
					foundDep = managedPath;
					depIsManaged = true;
				} else {
					var nativepath = $@"{probePath}\{found.path}\runtimes\{runtime.OS}-{runtime.Arch}\native";
					if (Directory.Exists(nativepath)) {
						foundDep = nativepath;
						depIsManaged = false;
					}
				}
			}

			/*
						var foundDep = runtime.PackageProbePaths
							.Select(x => {
								if(found.path.StartsWith("runtime."))
									return new DependencyPath { Path = Path.Combine(x, found.path, "runtimes", $"{runtime.OS}-{runtime.Arch}", "native"), IsManaged = false };
								else
									return new DependencyPath { Path = Path.Combine(x, found.path, "runtimes", runtime.OS, "lib", runtime.FrameworkVersion), IsManaged = true };
							})
							.FirstOrDefault(x => Directory.Exists(x.Path));
			*/
			if (foundDep != null)
				foreach(var item in Directory.GetFiles(foundDep))
				yield return new DependencyPath { Path = item, IsManaged = depIsManaged };

			var childDeps = deps.DepsFor(found.FullKey).ToList();
			//foreach(var item in childDeps)
			//	Console.WriteLine($"Resolving {item} for {depName}");

			foreach(var child in childDeps)
			foreach(var item in Resolve(deps, runtime, child.Split('/')[0], seen))
				yield return item;
		}

		static AssemblyJsonDeps Deps;

		class ConeLoadContext : AssemblyLoadContext
		{
			public void LoadNativeDll(string path) => LoadUnmanagedDllFromPath(path);

			protected override Assembly Load(AssemblyName assemblyName) => Default.LoadFromAssemblyName(assemblyName);
		}


		public static Assembly LoadFrom(string path) {
			var result = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
			if(Deps == null) {
				Deps = AssemblyJsonDeps.LoadFrom(path);
				var seen = new HashSet<string>();
				var dlls = new ConeLoadContext();
				AssemblyLoadContext.Default.Resolving += (_, e) => {
					//WriteDiagnostic($"Resolving {e.Name}");
					foreach(var item in Resolve(Deps, new RuntimeDep { 
							OS = Environment.GetEnvironmentVariable("CONE_OS") ?? "win",
							Arch = Environment.Is64BitProcess ? "x64" : "x86",
							//FrameworkVersion = "netstandard2.0",
							FrameworkVersion = Environment.GetEnvironmentVariable("CONE_TARGET_FRAMEWORK"),
							PackageProbePaths = AppContext.GetData("PROBING_DIRECTORIES").ToString().Split(new[]{ ';' }, StringSplitOptions.RemoveEmptyEntries),
					}, e.Name, seen)) { 
						//WriteDiagnostic($"  Loading Dependencty {item.Path} for {e.Name}");
						if(item.IsManaged)
							LoadFrom(item.Path);
						else dlls.LoadNativeDll(item.Path);
					}
					return null;
				};
			}
			return result;
		}

		static void WriteDiagnostic(string message) {
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
		}

		public class JsonDepsRuntimeTarget
		{
			public string name;
		}

		public class JsonTargetDep
		{
			public Dictionary<string, string> dependencies;
			public Dictionary<string, JsonRuntime> runtime;
			public Dictionary<string, JsonRuntimeTarget> runtimeTargets;
		}

		public class JsonRuntimeTarget
		{
			public string rid;
		}

		public class JsonRuntime
		{
			public string path;
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

		JsonDeps deps;

		public bool TryGetLibraryDep(string key, out JsonLibraryDep found) {
			foreach(var item in deps.libraries) {
				if(item.Key.Split('/')[0] == key) {
					found = item.Value;
					found.FullKey = item.Key;
					return true;
				}
			}
			found = null;
			return false;
		}

		public IEnumerable<string> DepsFor(string name) { 
			if(deps.targets[deps.runtimeTarget.name].TryGetValue(name, out var found) && found.dependencies != null) { 
				return found.dependencies.Select(x => x.Key);
			} else 
				return Enumerable.Empty<string>();
		}

		public static AssemblyJsonDeps LoadFrom(string assemblyPath) {
			var depsPath = Path.ChangeExtension(assemblyPath, DepsSuffix);
			var result = new AssemblyJsonDeps();
			if (File.Exists(depsPath)) {
				result.deps = JsonConvert.DeserializeObject<JsonDeps>(File.ReadAllText(depsPath));
			}
			return result;
		}

		public override string ToString() => JsonConvert.SerializeObject(deps, Formatting.Indented);
	}

	public class RuntimeDep
	{
		public string OS;
		public string Arch;
		public string FrameworkVersion;
		public string[] PackageProbePaths;
	}

}
