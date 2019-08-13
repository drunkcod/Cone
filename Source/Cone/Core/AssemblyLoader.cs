using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Cone.Runtime;

namespace Cone.Core
{
	using System.Diagnostics;
	using System.IO;
#if NETFX
    static class AssemblyLoader
    {
		public static Assembly LoadFrom(string path) => 
			Assembly.LoadFrom(path);

		public static void InitDeps(string mainAssembly) { }
    }

#else
	using System.Runtime.Loader;
	using Microsoft.DotNet.PlatformAbstractions;


	public class ConeLoadContext : AssemblyLoadContext
	{
		public TextWriter LoadingLog = TextWriter.Null;
		readonly Dictionary<AssemblyName, Assembly> knownAssemblies = new Dictionary<AssemblyName, Assembly>();

		public Assembly ResolveKnownAssembly(AssemblyLoadContext _, AssemblyName assemblyName) {
			if(knownAssemblies.TryGetValue(assemblyName, out var found))
				return found;
			return null;
		}

		public void Load(DependencyItem dep)
		{
			LoadingLog.WriteLine($"-> {dep.Path}");
			if(dep.IsManaged) {
				try { 
					Add(Default.LoadFromAssemblyPath(dep.Path));
				} catch(Exception ex) { 
					LoadingLog?.WriteLine($"!!! Managed Load Failure {dep.Path}: {ex}");	
				}
			} else try {
					LoadUnmanagedDllFromPath(dep.Path);
				}catch(Exception ex) {
					LoadingLog?.WriteLine($"!!! Native Load Failure {dep.Path}: {ex}");
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
			var os = Environment.GetEnvironmentVariable("CONE_RUNTIME_OS") ?? GetOs();
			var runtime = new RuntimeInfo {
				OS = os,
				Arch = RuntimeEnvironment.RuntimeArchitecture,
				//FrameworkVersion = "netstandard2.0",
				FrameworkVersion = Environment.GetEnvironmentVariable("CONE_TARGET_FRAMEWORK"),
				ApplicationBase = Path.GetDirectoryName(mainAssembly),
				PackageProbePaths = GetProbePaths(os),
			};
			//foreach(var path in runtime.PackageProbePaths)
			//	Console.WriteLine($"Probing: {path}");
			LoadContext = new ConeLoadContext { LoadingLog = TextWriter.Null };
			var seen = new HashSet<string>();
			foreach(var dep in deps.GetRuntimeDependencies()
				.SelectMany(x => x.GetLoadOrder())
				.Select(runtime.ResolveDependency)
				.Where(x => x.Items.Any()))
			if(seen.Add(dep.Name))
				Array.ForEach(dep.Items, LoadContext.Load);

			AssemblyLoadContext.Default.Resolving += LoadContext.ResolveKnownAssembly;
		}

		static string GetOs() {
			switch(RuntimeEnvironment.OperatingSystemPlatform) {
				case Platform.Windows: return "win";
				case Platform.Linux: return "linux";
				case Platform.Darwin: return "osx";
				default: throw new NotSupportedException();
			}
		}

		static string[] GetProbePaths(string os)
		{
			var probeDirs  = new HashSet<string>(
				AppContext.GetData("PROBING_DIRECTORIES")
				.ToString()
				.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));


			var packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
			if(!string.IsNullOrEmpty(packageDirectory))
				probeDirs.Add(packageDirectory);

			string basePath;
			if (os == "win") {
				basePath = Environment.GetEnvironmentVariable("USERPROFILE");
			}
			else {
				basePath = Environment.GetEnvironmentVariable("HOME");
			}
			if(basePath != null)
				probeDirs.Add(Path.Combine(basePath, ".nuget", "packages"));

			return probeDirs.ToArray();
		}
	}
#endif
}
