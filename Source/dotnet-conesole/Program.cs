using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
					allOk &= RunConesole(item.TargetFramework, item.GetTargetPath()) == 0;					
				}
				return allOk ? 0 : -1;
			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
		}

		static int RunConesole(string fxVersion, params string[] args)
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
			foreach(var toolPath in probePaths.Select(x => GetWorkerProbe(x, fxVersion))) {
				if(!File.Exists(toolPath.worker))
					continue;
				var conesole = Process.Start(new ProcessStartInfo { 
					FileName = toolPath.isDll ? "dotnet" : toolPath.worker,	
					Arguments = (toolPath.isDll ? toolPath.worker + " " : string.Empty) + string.Join(' ', Array.ConvertAll(args, x => $"\"{x}\""))
				});
				conesole.WaitForExit();
				return conesole.ExitCode;
			}
			throw new Exception("Failed to locate Cone.Worker");
		}

		static (string worker, bool isDll) GetWorkerProbe(string probePath, string fxVersion) {
			switch(fxVersion) {
				case "net45": 
				case "net451": 
				case "net452":
				case "net46":
				case "net461":
				case "net462":
				case "net47":
				case "net471":
				case "net472":
					return (Path.Combine(probePath, "net45", "Cone.Worker.exe"), false);
			}
			return (Path.Combine(probePath, "netcoreapp2.0", "Cone.Worker.dll"), true);
		}

		static TargetCollection GetTargetInfo() {
			var tmp = Path.GetTempFileName();
			try {
				var msbuild = Process.Start(new ProcessStartInfo { 
					FileName = "dotnet",
					Arguments = $"msbuild {FindTargetProject()} /nologo /p:Cone-TargetFile={tmp} /p:CopyLocalLockFileAssemblies=true /t:Build,Cone-TargetInfo"
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
