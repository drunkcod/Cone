using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using CheckThat.Internals;
using Cone.Core;
using Cone.Runners;
using Cone.Worker;

namespace Conesole
{
	class Program : MarshalByRefObject
	{
		public string[] AssemblyPaths;
		public string[] Options;

		static int Main(string[] args) {
			if(args.Length == 0)
				return DisplayUsage();
			Console.OutputEncoding = Encoding.UTF8;
			string configPath = null;
			string[] assemblyPaths;
			try {
				var config = WorkerConfiguration.Parse(args);
				configPath = config.ConfigPath;
				assemblyPaths = config.AssemblyPaths;
			} catch {
				return DisplayUsage();
			}

			if(args.Contains("--debug"))
				Debugger.Launch();

			if(!args.Contains("--autotest"))
				return RunTests(args, assemblyPaths, configPath);

			var q = new CircularQueue<string>(32);
			var watchers = assemblyPaths.ConvertAll(path => {
				var watcher = new FileSystemWatcher {
					Path = Path.GetDirectoryName(path),
					Filter = Path.GetFileName(path),
				};
			
				watcher.Changed += (_, e) => {
					while(!q.TryEnqueue(e.FullPath))
						Thread.Yield();
				};
				watcher.EnableRaisingEvents = true;
				return watcher;

			});

			Console.WriteLine("Running in autotest mode, press any key to quit.");

			ParameterizedThreadStart workerRunTests = x => {
				var paths = (string[])x;
				Console.WriteLine("[{0}] Change(s) detected in: {1}\n\t", DateTime.Now.ToString("HH:mm:ss"), string.Join("\n\t", paths));
				RunTests(args, paths, configPath);
			};
			var worker = new Thread(workerRunTests);
			worker.Start(assemblyPaths);

			var changed = new HashSet<string>(); 
			var cooldown = Stopwatch.StartNew();
			do {
				string value;
				while (q.TryDeque(out value)) { 
					changed.Add(value);
					cooldown.Restart();
				}
				if(changed.Count > 0 && worker.Join(25) && cooldown.Elapsed.TotalMilliseconds > 100) {
					worker = new Thread(workerRunTests);
					worker.Start(changed.ToArray());
					changed.Clear();
				} else Thread.Sleep(25);
			} while(!Console.KeyAvailable);
			worker.Join();
			return 0;
		}

		private static int RunTests(string[] args, string[] assemblyPaths, string configPath) {
			if(!string.IsNullOrEmpty(configPath))
				return CrossDomainRun(args, assemblyPaths, configPath);
			return OutOfProcRun(args);
		}

		private static int CrossDomainRun(string[] args, string[] assemblyPaths, string configPath) {
			return CrossDomainConeRunner.WithProxyInDomain<Program, int>(
				configPath,
				assemblyPaths,
				program => {
					program.AssemblyPaths = assemblyPaths;
					program.Options = args;
					return program.Execute();
				});
		}

		private static int OutOfProcRun(string[] args) {
			var r = args.Aggregate(new StringBuilder(), (acc, x) => acc.AppendFormat("\"{0}\" ", x.Replace("\"", "\"\"\"")));
			r.Length -= 1;
			var p = Process.Start(new ProcessStartInfo {
				UseShellExecute = false,
				FileName = $@"{ExePath}\Bin\Cone.Worker.exe",
				Arguments = r.ToString(),
			});
			p.WaitForExit();
			return p.ExitCode;
		}

		static string ExePath => Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).LocalPath);

		int Execute() {
			try {
				var config = WorkerConfiguration.Parse(Options);
				var logger = CreateLogger(config);
				var results = CreateTestSession(logger, config);
				if(config.IsDryRun)
					results.GetTestExecutor = _ => new DryRunTestExecutor();
				var runner = new SimpleConeRunner(new ConeTestNamer()) {
					Workers = config.Multicore ? Environment.ProcessorCount : 1,
				};
				if(!config.NoLogo)
					logger.WriteInfo(x => x.Info("Cone {0}\n", runner.GetType().Assembly.GetName().Version.ToString(3)));
				var assemblies = CrossDomainConeRunner.LoadTestAssemblies(AssemblyPaths, Error);
				if (config.RunList == null)
					runner.RunTests(results, assemblies);
				else runner.RunTests(config.RunList, results, assemblies);
				results.Report();
				return results.FailureCount;
			} catch (ReflectionTypeLoadException tle) {
				foreach (var item in tle.LoaderExceptions)
					Error("{0}\n---", item);
				return -1;
			} catch(ArgumentException e) {
				Error(e.Message);
				return DisplayUsage();
			} catch (Exception e) {
				Error(e);
				return -1;
			}
		}

		void Error(string message) { Console.Error.WriteLine(message); }
		void Error(string format, params object[] args) { Console.Error.WriteLine(format, args); }
		void Error(Exception e) { Console.Error.WriteLine(e); }

		static TestSession CreateTestSession(ISessionLogger logger, WorkerConfiguration config) {
			return new TestSession(logger) {
				IncludeSuite = config.IncludeSuite,
				ShouldSkipTest = x => !config.IncludeTest(x)
			};
		}

		static ISessionLogger CreateLogger(WorkerConfiguration config) {
			var loggers = new List<ISessionLogger>();

			if (config.XmlConsole) {
				loggers.Add(new XmlSessionLogger(new XmlTextWriter(Console.Out){
					Formatting = Formatting.Indented
				}));
			} 
			else if(config.TeamCityOutput)
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
			var encoding = Encoding.UTF8;
			Uri remoteLocation;
			if(!Uri.TryCreate(path, UriKind.Absolute, out remoteLocation))
				return new XmlSessionLogger(new XmlTextWriter(path, encoding) {
					Formatting = Formatting.Indented
				});
			
			var output = new MemoryStream();
			var xmlLogger = new XmlSessionLogger(new XmlTextWriter(output, encoding) {
				Formatting = Formatting.Indented
			});

			xmlLogger.SessionEnded += (_, __) => {
				using(var http = new HttpClient()) {
					var body = new ByteArrayContent(output.ToArray());
					body.Headers.ContentType = new MediaTypeHeaderValue("text/xml") {
						CharSet = encoding.WebName,
					};
					try {
						http.PostAsync(remoteLocation, body).Wait();
					} catch { 
						Console.Error.WriteLine("\nPOST to " + remoteLocation + " failed.");
					}
				}
			};

			return xmlLogger;
		}

		static int DisplayUsage() {
			using(var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Conesole.Usage.txt"))) {
				Console.WriteLine(reader.ReadToEnd());
			}
			return -1;
		}
	}
}
