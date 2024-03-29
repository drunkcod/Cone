using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

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

	class Program
	{
		class CommandSettings
		{
			public bool NoBuild;
		}

		static bool TryParseCommandArgs(IEnumerable<string> args, out CommandSettings result) {
			var s = new CommandSettings();
			foreach(var item in args)
				switch(item) {
					default:
						result = null;
						return false;
					case "--no-build": 
						s.NoBuild = true;
						break;
				}
			result = s;
			return true;
		}

		static int Main(string[] args) {
			var runSettings = new List<string>();
			var commandArgs = new List<string>();

			var target = commandArgs;
			foreach(var item in args)
				if(item == "--")
					target = runSettings;
				else target.Add(item);

			if(!TryParseCommandArgs(commandArgs, out var settings))
			{
				Console.WriteLine("usage is: dotnet conesole [--no-build] [-- <conesole settings>]");
				return -1;
			}

			try { 
				var targetInfo = GetTargetInfo(settings);
				var allOk = true;
				runSettings.Insert(0, string.Empty);
				foreach(var item in targetInfo) {
					runSettings[0] = item.GetTargetPath();
					allOk &= RunConesole(item.TargetFramework, runSettings) == 0;					
				}
				return allOk ? 0 : -1;
			} catch(Exception ex) {
				Console.Error.WriteLine("Failed to run specs: {0}", ex);
				return -1;
			}
		}

		static int RunConesole(string fxVersion, IEnumerable<string> args)
		{
			var myPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			var probePaths = new [] {
				Path.Combine(myPath, "..", "..", "tools"),
#if DEBUG
				Path.Combine(myPath, "..", "..", "..", "Cone.Worker", "Debug"),
#endif
			};

			foreach(var (worker, isDll) in probePaths.Select(x => GetWorkerProbe(x, fxVersion))) {
				if(!File.Exists(worker))
					continue;

				var startInfo = new ProcessStartInfo {
					FileName = isDll ? "dotnet" : worker,
					Arguments = (isDll ? worker + " " : string.Empty) + string.Join(' ', args.Select(x => $"\"{x}\"")),
				};
				startInfo.Environment.Add("CONE_TARGET_FRAMEWORK", fxVersion);
				var conesole = Process.Start(startInfo);
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
					return (Path.Combine(probePath, "net452", "Cone.Worker.exe"), false);
				case "net46":
				case "net461":
				case "net462":
					return (Path.Combine(probePath, "net462", "Cone.Worker.exe"), false);
				case "net47":
				case "net471":
				case "net472":
					return (Path.Combine(probePath, "net472", "Cone.Worker.exe"), false);
				case "netcoreapp2.0":
				case "netcoreapp2.1":
					return (Path.Combine(probePath, "netcoreapp2.1", "Cone.Worker.dll"), true);
			}
			return (Path.Combine(probePath, "netcoreapp2.2", "Cone.Worker.dll"), true);
		}

		static IEnumerable<TargetInfo> GetTargetInfo(CommandSettings settings) {
			var tmp = Path.GetTempFileName();
			try {
				var build = settings.NoBuild ? string.Empty : "/t:Build";
				var msbuild = Process.Start(new ProcessStartInfo { 
					FileName = "dotnet",
					Arguments = $"msbuild {FindTargetProject()} /nologo {build} /t:Cone-TargetInfo /p:Cone-TargetFile={tmp} /p:CopyLocalLockFileAssemblies=true"
				});
				msbuild.WaitForExit();
				var targets = new List<TargetInfo>();
				using(var info = new JsonTextReader(File.OpenText(tmp)) { SupportMultipleContent = true }) {
					var json = new JsonSerializer();
					while(info.Read())
						targets.Add(json.Deserialize<TargetInfo>(info));
				}
				return targets;
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
