using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Conesole.NetCoreApp
{
	public class TargetInfo
	{
		public string ProjectName;
		public string OutputPath;
		public string TargetFileName;
		public string TargetFrameworks;

		public string[] GetTargetFrameworks() => TargetFrameworks.Split(',');
		public string GetTargetPath() => Path.Combine(OutputPath, TargetFileName);
	}

	class Program
	{
		static string[] DesktopFrameworks = new[] {
			"net45",
			"net451",
			"net452",
			"net462",
			"net47",
			"net471",
		};

		static int Main(string[] args) {
			if(args.Length > 0)
			{
				Console.WriteLine("usage is: dotnet conesole");
				return -1;
			}
			try { 
				var targetInfo = GetTargetInfo();
				foreach(var fx in targetInfo.GetTargetFrameworks()) {
//					Console.WriteLine("Running specs from {0} targeting {1}", targetInfo.TargetProject, fx);
					return IsDesktopFramework(fx) 
					? RunDesktopConesole(targetInfo.GetTargetPath())
					: RunInProcConesole(args);
				}
				return 0;
			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
		}

		static int RunDesktopConesole(params string[] args)
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

		static bool IsDesktopFramework(string targetFramework) => DesktopFrameworks.Contains(targetFramework);

		static TargetInfo GetTargetInfo() {
			var tmp = Path.GetTempFileName();
			try {
				var msbuild = Process.Start(new ProcessStartInfo { 
					FileName = "dotnet",
					Arguments = $"msbuild {FindTargetProject()} /nologo /p:Cone-TargetFile={tmp} /t:Build,Cone-TargetInfo"
				});
				msbuild.WaitForExit();
				using(var info = File.OpenRead(tmp)) {
					var xml = new XmlSerializer(typeof(TargetInfo));
					var result = (TargetInfo)xml.Deserialize(XmlReader.Create(info, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }));
					//result.TargetProject = proj;
					return result;
				}
			} catch(Exception ex) {
				throw new InvalidOperationException("Failed to detect project", ex);
			}
			finally {
				File.Delete(tmp);
			}
		}

		static string FindTargetProject() {
			var proj = Directory.GetFiles(".", "*.csproj");
			switch(proj.Length) {
				case 0: throw new Exception("No .csproj found.");
				case 1: return proj[0];
				default: throw new Exception("More than one .csproj found.");
			}

		}
	}
}
