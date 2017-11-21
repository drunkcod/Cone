using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Conesole.NetCoreApp
{
    class Program
    {
        static int Main(string[] args) {
			if(args.Length != 1)
			{
				Console.WriteLine("usage is: dotnet conesole <path-to-spec-assembly>");
				return -1;
			}
			try { 
				return IsDesktopFramework(GetTargetFramework(args[0])) 
				? RunDesktopConesole(args)
				: RunInProcConesole(args);

			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
        }

		static int RunDesktopConesole(string[] args)
		{
			var myPath = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).LocalPath);
			var probePaths = new [] {
				Path.Combine(myPath, "..", "..", "tools"),
				Path.Combine(myPath, "..", "..", "..", "Conesole", "Debug"),
			};
			var toolPath = probePaths.Select(x => Path.Combine(Path.GetFullPath(x), "net45", "Conesole.exe")).FirstOrDefault(File.Exists);
			if(toolPath == null)
				throw new Exception("Failed to locate Conesole.exe");
			var conesole = Process.Start(new ProcessStartInfo { 
				FileName = toolPath,
				Arguments = string.Join(' ', Array.ConvertAll(args, x => $"\"{x}\""))
			});
			conesole.WaitForExit();
			return conesole.ExitCode;
		}

		static int RunInProcConesole(string[] args) {
			var specs = Assembly.LoadFrom(args[0]);
			var localCone = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(new Uri(specs.CodeBase).LocalPath), "Cone.dll"));
			var inProcRunnerType = localCone.GetType("Cone.Runners.InProcRunner");
			var inProcRunner = inProcRunnerType.GetConstructor(Type.EmptyTypes).Invoke(null);
			return (int)inProcRunnerType.GetMethod("Main").Invoke(inProcRunner, new[]{ args });
		}

		static bool IsDesktopFramework(string targetFramework) => targetFramework.StartsWith(".NETFramework");

		static string GetTargetFramework(string assembly) {
			string frameworkName;
			try { 
				var specs = Assembly.LoadFrom(assembly);
				if(TryGetFrameworkName(specs, out frameworkName))
					return frameworkName;

				var localCone = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(new Uri(specs.CodeBase).LocalPath), "Cone.dll"));
				if(TryGetFrameworkName(localCone, out frameworkName))
					return frameworkName;
			} catch(BadImageFormatException) { }

			TryGetFrameworkName(typeof(Program).Assembly, out frameworkName);
			return frameworkName;
		}

		static bool TryGetFrameworkName(ICustomAttributeProvider attrs, out string found) {
			foreach (var item in attrs.GetCustomAttributes(true)) {
				var t = item.GetType();
				if (t.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute") { 
					found = (string)t.GetProperty("FrameworkName").GetValue(item);
					return true;
				}
			}
			found = null;
			return false;
		}
	}
}
