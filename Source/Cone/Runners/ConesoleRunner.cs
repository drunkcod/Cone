using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETFX
#else
using System.Runtime.Loader;
#endif
using System.Text;
using System.Xml;
using Cone.Core;
using Cone.Worker;

namespace Cone.Runners
{
	public class ConesoleRunner
	{
		public static int Main(string[] args) {
			var config = WorkerConfiguration.Parse(args);
			var logger = CreateLogger(config);
			var results = CreateTestSession(logger, config);
			if (config.IsDryRun)
				results.GetTestExecutor = _ => new DryRunTestExecutor();
			var runner = new SimpleConeRunner(new ConeTestNamer()) {
				Workers = config.Multicore ? Environment.ProcessorCount : 1,
			};

			if (!config.NoLogo)
				logger.WriteInfo(x => {
					x.Info("Cone {0} on {1}\n", runner.GetType().Assembly.GetName().Version.ToString(3), GetRuntimeHostName());
					x.Write("  " + string.Join(", ", config.AssemblyPaths) + "\n");
				});

			var specPath = config.AssemblyPaths[0];
			var specBinPath = Path.GetDirectoryName(specPath);
			Environment.CurrentDirectory = specBinPath;
			AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
				var probeName = e.Name.Split(',').First();
				foreach(var ext in new[] { ".dll", ".exe "}) {
					var probeBin = Path.Combine(specBinPath, probeName + ext);
					if(File.Exists(probeBin))
						return AssemblyLoader.LoadFrom(probeBin);
				}
				return null;
			};
			var assemblies = Array.ConvertAll(config.AssemblyPaths, AssemblyLoader.LoadFrom);
			if (config.RunList == null)
				runner.RunTests(results, assemblies);
			else runner.RunTests(config.RunList, results, assemblies);
			
			results.Report();
			return results.FailureCount;
		}

		static string GetRuntimeHostName() {
			var pathParts = Path.GetFullPath(new Uri(typeof(object).Assembly.CodeBase).LocalPath).Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			var n = Array.IndexOf(pathParts, "Microsoft.NETCore.App");
			if(n != -1)
				return $"{pathParts[n]} {pathParts[n + 1]}";
			return $".NET Framework {Version.Parse(((AssemblyFileVersionAttribute)typeof(object).Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version).ToString(3)}";
		}

		static TestSession CreateTestSession(ISessionLogger logger, WorkerConfiguration config) {
			return new TestSession(logger) {
				IncludeSuite = config.IncludeSuite,
				ShouldSkipTest = x => !config.IncludeTest(x)
			};
		}

		static ISessionLogger CreateLogger(WorkerConfiguration config) {
			var loggers = new List<ISessionLogger>();

			if (config.XmlConsole) {
				loggers.Add(new XmlSessionLogger(new XmlTextWriter(Console.Out) {
					Formatting = Formatting.Indented
				}));
			}
			else if (config.TeamCityOutput)
				loggers.Add(new TeamCityLogger(Console.Out));
			else {
				var settings = new ConsoleLoggerSettings {
					Verbosity = config.Verbosity,
					SuccessColor = config.IsDryRun
						? ConsoleColor.DarkGreen
						: ConsoleColor.Green,
					Multicore = config.Multicore,
					ShowTimings = config.ShowTimings,
				};
				loggers.Add(new ConsoleSessionLogger(settings));
			}

			if (!string.IsNullOrEmpty(config.XmlOutput)) {
				loggers.Add(CreateXmlLogger(config.XmlOutput));
			}
			return loggers.Count == 1
				? loggers[0]
				: new MulticastSessionLogger(loggers);
		}

		private static XmlSessionLogger CreateXmlLogger(string path) {
			return new XmlSessionLogger(new XmlTextWriter(path, Encoding.UTF8) {
				Formatting = Formatting.Indented
			});
		}
	}
}