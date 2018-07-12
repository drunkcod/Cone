using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Cone.Runtime;

namespace Cone.Core
{
	using System.IO;
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


	public class ConeLoadContext : AssemblyLoadContext
	{
		readonly TextWriter LoadingLog = TextWriter.Null;
		readonly Dictionary<AssemblyName, Assembly> knownAssemblies = new Dictionary<AssemblyName, Assembly>();

		public Assembly ResolveKnownAssembly(AssemblyLoadContext parent, AssemblyName assemblyName) {
			LoadingLog?.WriteLine($"Resolving {assemblyName}");
			knownAssemblies.TryGetValue(assemblyName, out var found);
			return found;
		}

		public void Load(DependencyItem dep)
		{
			if(dep.IsManaged) {
				try { 
					var loaded = Default.LoadFromAssemblyPath(dep.Path);
					knownAssemblies.Add(loaded.GetName(), loaded);
				} catch(Exception ex) { 
					LoadingLog?.WriteLine($"Ooops when loading {dep.Path}");	
				}
			} else try {
					LoadUnmanagedDllFromPath(dep.Path);
				}catch(Exception ex) {
					LoadingLog?.WriteLine($"Ooops when loading {dep.Path}");
				}
		}

		public void Add(Assembly assembly) => knownAssemblies.Add(assembly.GetName(), assembly);

		protected override Assembly Load(AssemblyName assemblyName) {
			if(knownAssemblies.TryGetValue(assemblyName, out var found))
				return found;
			return Default.LoadFromAssemblyName(assemblyName);
		}
	}

	public static class AssemblyLoader
	{		
		static ConeLoadContext LoadContext;

		public static Assembly LoadFrom(string path) {
			if (LoadContext == null)
				InitDeps(path);
			return LoadContext.LoadFromAssemblyPath(path);
		}

		public static void InitDeps(string mainAssembly) {
			var deps = AssemblyJsonDeps.LoadFrom(mainAssembly);
			var runtime = new RuntimeInfo {
				OS = Environment.GetEnvironmentVariable("CONE_RUNTIME_OS") ?? (string)typeof(RuntimeEnvironment).GetMethod("GetRIDOS", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null),
				Arch = RuntimeEnvironment.RuntimeArchitecture,
				//FrameworkVersion = "netstandard2.0",
				FrameworkVersion = Environment.GetEnvironmentVariable("CONE_TARGET_FRAMEWORK"),
				PackageProbePaths = AppContext.GetData("PROBING_DIRECTORIES").ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
			};
			LoadContext = new ConeLoadContext();
			var seen = new HashSet<string>();
			foreach(var dep in deps.GetRuntimeDependencies()
				.SelectMany(x => x.GetLoadOrder())
				.Select(runtime.ResolveDependency)
				.Where(x => x.Items.Any()))
			if(seen.Add(dep.Name))
				Array.ForEach(dep.Items, LoadContext.Load);

			AssemblyLoadContext.Default.Resolving += LoadContext.ResolveKnownAssembly;
		}
	}
#endif
}
