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
		public string TargetFramework;

		public string GetTargetPath() => Path.Combine(OutputPath, TargetFileName);
	}

	[XmlRoot("Targets")]
	public class TargetCollection
	{
		[XmlElement("Target")]
		public TargetInfo[] Items;
	}

	class Program
	{
		static int Main(string[] args) {
			if(args.Length > 0)
			{
				Console.WriteLine("usage is: dotnet conesole");
				return -1;
			}
			try { 
				var targetInfo = GetTargetInfo();
				var allOk = true;
				foreach(var item in targetInfo.Items) {
					allOk &= RunDesktopConesole(item.TargetFramework, item.GetTargetPath()) == 0;					
				}
				return allOk ? 0 : -1;
			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
		}

		static int RunDesktopConesole(string fxVersion, params string[] args)
		{
			var myPath = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).LocalPath);
			var probePaths = new [] {
				Path.Combine(myPath, "..", "..", "tools"),
				Path.Combine(myPath, "..", "..", "..", "Cone.Worker", "Debug"),
			};
			var workers = new[] {
				"Cone.Worker.exe",
				"Cone.Worker.dll"
			};
			var toolPath = probePaths.SelectMany(x => workers.Select(worker => new { path = x, worker })).Select(x => Path.Combine(Path.GetFullPath(x.path), fxVersion, x.worker)).Select(x => 
			{
				Console.WriteLine("Probing {0}", x);
				return x;
			}).FirstOrDefault(File.Exists);
			if(toolPath == null)
				throw new Exception("Failed to locate Cone.Worker");
			var isDll = toolPath.EndsWith(".dll");
			var conesole = Process.Start(new ProcessStartInfo { 
				FileName = isDll ? "dotnet" : toolPath,	
				Arguments = (isDll ? toolPath + " " : string.Empty) + string.Join(' ', Array.ConvertAll(args, x => $"\"{x}\""))
			});
			conesole.WaitForExit();
			return conesole.ExitCode;
		}

		static TargetCollection GetTargetInfo() {
			var tmp = Path.GetTempFileName();
			try {
				var msbuild = Process.Start(new ProcessStartInfo { 
					FileName = "dotnet",
					Arguments = $"msbuild {FindTargetProject()} /nologo /p:Cone-TargetFile={tmp} /t:Build,Cone-TargetInfo"
				});
				msbuild.WaitForExit();
				//Console.WriteLine(File.ReadAllText(tmp));
				using(var info = File.OpenRead(tmp)) {
					var xml = new XmlSerializer(typeof(TargetCollection));
					var result = (TargetCollection)xml.Deserialize(XmlReader.Create(info, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }));
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
