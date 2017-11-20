using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Conesole.CoreApp
{
    class Program
    {
        static int Main(string[] args) {
			try { 
				var a = Assembly.LoadFrom(args[0]);
				var targetFramework = GetTargetFramework(a);
				if(IsDesktopFramework(targetFramework)) {
					return RunDesktopConesole(args);
				} else { 
					Console.WriteLine($"{targetFramework} support pending.");
					return -1;
				}
			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
        }

		static int RunDesktopConesole(string[] args)
		{
			var myPath = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).LocalPath);
			var probePaths = new [] {
				Path.Combine(myPath, ".."),
				Path.Combine(myPath, "..", "..", "..", "Conesole", "Debug"),
			};
			var toolPath = probePaths.Select(x => Path.Combine(Path.GetFullPath(x), "net45", "Conesole.exe")).FirstOrDefault(File.Exists);
			if(toolPath == null)
				throw new Exception("Failed to locate Conesole.exe");
			Console.WriteLine("Running from {0}", toolPath);
			var conesole = Process.Start(new ProcessStartInfo { 
				FileName = toolPath,
				Arguments = string.Join(' ', Array.ConvertAll(args, x => $"\"{x}\""))
			});
			conesole.WaitForExit();
			return conesole.ExitCode;
		}

		static bool IsDesktopFramework(string targetFramework) => targetFramework.StartsWith(".NETFramework");

		static string GetTargetFramework(Assembly specs) {
			var localCone = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(new Uri(specs.CodeBase).LocalPath), "Cone.dll"));
			foreach (var item in localCone.GetCustomAttributes()) {
				var t = item.GetType();
				if (t.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
					return (string)t.GetProperty("FrameworkName").GetValue(item);
			}
			return ".NETCoreApp,Version=2.0";
		}
	}
}
