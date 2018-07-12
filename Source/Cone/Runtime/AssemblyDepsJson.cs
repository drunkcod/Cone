using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cone.Runtime
{
	public class AssemblyJsonDeps
	{
		const string DepsSuffix = "deps.json";

		public class JsonDeps
		{
			static Dictionary<string, JsonTargetDep> NoDeps = new Dictionary<string, JsonTargetDep>();

			public JsonDepsRuntimeTarget runtimeTarget;
			public Dictionary<string, Dictionary<string, JsonTargetDep>> targets = new Dictionary<string, Dictionary<string, JsonTargetDep>>();
			public Dictionary<string, JsonLibraryDep> libraries = new Dictionary<string, JsonLibraryDep>();

			public Dictionary<string, JsonTargetDep> CurrentRuntimeDeps => runtimeTarget == null ? NoDeps : targets[runtimeTarget.name];
		}

		public class JsonDepsRuntimeTarget
		{
			public string name;
		}

		public JsonDeps deps;
		readonly Dictionary<string, JsonTargetDep> targetLookup = new Dictionary<string, JsonTargetDep>();
		readonly Dictionary<string, JsonLibraryDep> libraryLookup = new Dictionary<string, JsonLibraryDep>();

		public bool TryGetLibraryDep(string key, out JsonLibraryDep found) => libraryLookup.TryGetValue(key, out found);

		public bool TryGetTarget(string name, out JsonTargetDep found) => targetLookup.TryGetValue(name, out found);

		public IEnumerable<string> DepsFor(string name) {
			if (deps.CurrentRuntimeDeps.TryGetValue(name, out var found) && found.dependencies != null) {
				return found.dependencies.Select(x => x.Key);
			}
			else
				return Enumerable.Empty<string>();
		}

		public static AssemblyJsonDeps LoadFrom(string assemblyPath) {
			var depsPath = Path.ChangeExtension(assemblyPath, DepsSuffix);
			var result = new AssemblyJsonDeps();
			if (File.Exists(depsPath)) {
				result.deps = JsonConvert.DeserializeObject<JsonDeps>(File.ReadAllText(depsPath));

				foreach (var item in result.deps.targets[result.deps.runtimeTarget.name]) {
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

		public IEnumerable<RuntimeDependencyInfo> GetRuntimeDependencies() {
			var seen = new Dictionary<string, RuntimeDependencyInfo>();
			return deps.libraries.Keys.Select(x => GetRuntimeDependencyInfo(x, seen));
		}

		RuntimeDependencyInfo GetRuntimeDependencyInfo(string key, Dictionary<string, RuntimeDependencyInfo> lookupCache) {
			if(lookupCache.TryGetValue(key, out var found))
				return found;
			var x = new { Key = key, Value = deps.libraries[key] };
			var myDeps = deps.CurrentRuntimeDeps[x.Key];
			found = new RuntimeDependencyInfo {
				Name = x.Key,
				Type = x.Value.type,
				IsServiceable = x.Value.serviceable,
				Path = x.Value.path ?? string.Empty,
				Dependencies = myDeps.dependencies.Select(y => GetRuntimeDependencyInfo($"{y.Key}/{y.Value}", lookupCache)).ToArray(),
				Runtime = myDeps.runtime.Select(y => new RuntimePackageReference {
					Path = y.Key,
					AssemblyVersion = y.Value.assemblyVersion,
					FileVersion = y.Value.fileVersion,
				}).ToArray(),
				RuntimeTarget = myDeps.runtimeTargets.Select(y => new RuntimeTargetInfo { 
					Path = y.Key,
					AssetType = y.Value.assetType,
					Rid = y.Value.rid,
				}).ToArray(),
			};
			lookupCache.Add(key, found);
			return found;
		}

		public static string NameOnly(string nameVersion) => nameVersion.Substring(0, nameVersion.IndexOf('/'));

		public override string ToString() => JsonConvert.SerializeObject(deps, Formatting.Indented);
	}

	public class RuntimeDependencyInfo
	{
		public string Name;
		public string Type;
		public bool IsServiceable;
		public string Path;
		public RuntimeDependencyInfo[] Dependencies;
		public RuntimePackageReference[] Runtime;
		public RuntimeTargetInfo[] RuntimeTarget;

		public IEnumerable<RuntimeDependencyInfo> GetLoadOrder() {
			foreach(var item in Dependencies.SelectMany(x => x.GetLoadOrder()).Distinct())
				yield return item;
			yield return this;
		}
	}

	public class RuntimeInfo
	{
		public string OS;
		public string Arch;
		public string FrameworkVersion;
		public string ApplicationBase;
		public string[] PackageProbePaths;

		public string RID => $"{OS}-{Arch}";

		public DependencyResolutionItem ResolveDependency(RuntimeDependencyInfo dep) {
			var runtimeTargets = dep
					.RuntimeTarget
					.Join(new[] { 
						new { Rid = RID, SortOrder = 1 },
						new { Rid = OS, SortOrder = 2 },
					}, x => x.Rid, x => x.Rid, (x, n) => new { Target = x, n.SortOrder })
					.OrderBy(x => x.SortOrder)
					.Select(x => new DependencyItem(x.Target.Path, x.Target.IsManaged))
					.ToList();

			var loadTargets = runtimeTargets.Any() ? runtimeTargets : dep.Runtime.Select(x => new DependencyItem(x.Path, true)).ToList();
			if(!loadTargets.Any())
				return new DependencyResolutionItem { Name = dep.Name, Items = new DependencyItem[0] };

			var items = new DependencyItem[loadTargets.Count];
			var itemIndex = 0;
			foreach (var candidate in loadTargets)
				if (string.IsNullOrEmpty(candidate.Path)) {
					var managedPath = $@"{ApplicationBase}\{candidate.Path}";
					var found = File.Exists(managedPath);
					if (found) {
						items[itemIndex++] = new DependencyItem(managedPath, candidate.IsManaged);
					}
				}
				else
					foreach (var probePath in PackageProbePaths) {
						var managedPath = $@"{probePath}\{dep.Path}\{candidate.Path}";
						var found = File.Exists(managedPath);
						if (found) {
							items[itemIndex++] = new DependencyItem(managedPath, candidate.IsManaged);
							break;
}
					}
			Array.Resize(ref items, itemIndex);
			return new DependencyResolutionItem { Name = dep.Name, Items = items, };
		}
	}

	public struct DependencyResolutionItem
	{
		public string Name;
		public DependencyItem[] Items;
	}

	public struct DependencyItem
	{
		public readonly string Path;
		public readonly bool IsManaged;

		public DependencyItem(string path, bool isManaged) {
			this.Path = path;
			this.IsManaged = isManaged;
		}
	}


	public struct RuntimePackageReference
	{
		public string Path;
		public string AssemblyVersion;
		public string FileVersion;
	}

	public struct RuntimeTargetInfo
	{
		public string Path;
		public string AssetType;
		public string Rid;
		public bool IsManaged => AssetType == "runtime";
	}


	public class JsonLibraryDep
	{
		public string FullKey;
		public string type;
		public bool serviceable;
		public string sha512;
		public string path;
		public string hashPath;
	}

	public class JsonTargetDep
	{
		public string FullKey;
		public Dictionary<string, string> dependencies = new Dictionary<string, string>();
		public Dictionary<string, JsonRuntimeVersionInfo> runtime = new Dictionary<string, JsonRuntimeVersionInfo>();
		public Dictionary<string, JsonRuntimeTarget> runtimeTargets = new Dictionary<string, JsonRuntimeTarget>();

	}

	public struct JsonRuntimeTarget
	{
		public string rid;
		public string assetType;
	}

	public struct JsonRuntimeVersionInfo
	{
		public bool IsEmpty => string.IsNullOrEmpty(assemblyVersion) && string.IsNullOrEmpty(fileVersion);
		public string assemblyVersion;
		public string fileVersion;
	}
}
