using Cone.Core;
using Cone.Runners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Conesole
{

	public class ConesoleConfiguration
	{	
		class ConesoleFilterConfiguration
		{
			static Regex CreatePatternRegex(string pattern) {
				return new Regex("^" + pattern
					.Replace("\\", "\\\\")
					.Replace(".", "\\.")
					.Replace("*", ".*?"));
			}
		
			readonly List<string> includedCategories = new List<string>();
			readonly List<string> excludedCategories = new List<string>();

			Predicate<IConeTest> testFilter;
			Predicate<IConeSuite> suiteFilter;

			public bool Include(IConeTest test) { return CategoryCheck(test) && (testFilter == null || testFilter(test)); }
			public bool Include(IConeSuite suite) { return CategoryCheck(suite) && (suiteFilter == null || suiteFilter(suite)); }  
	
			public void IncludeTests(string value) {
				var suitePattern = "*";
				var parts = value.Split('.');

				if(parts.Length > 1)
					suitePattern = string.Join(".", parts, 0, parts.Length - 1);
					
				var testPatternRegex = CreatePatternRegex(suitePattern + "." + parts.Last());
				var suitePatternRegex = CreatePatternRegex(suitePattern);

				suiteFilter = suiteFilter.Or(x => suitePatternRegex.IsMatch(x.Name));
				testFilter = testFilter.Or(x => testPatternRegex.IsMatch(x.Name));
			}

			public void Categories(string value) {
				foreach(var category in value.Split(','))
					if(category.StartsWith("!"))
						excludedCategories.Add(category.Substring(1));
					else
						includedCategories.Add(category);
			}

			bool CategoryCheck(IConeEntity entity) {
				return (includedCategories.IsEmpty() || entity.Categories.Any(includedCategories.Contains)) 
					&& !entity.Categories.Any(excludedCategories.Contains);
			}
		}

		const string OptionPrefix = "--";
		static readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix), RegexOptions.Compiled);

		readonly ConesoleFilterConfiguration filters = new ConesoleFilterConfiguration();

		public bool IncludeTest(IConeTest test) { return filters.Include(test); }
		public bool IncludeSuite(IConeSuite suite) { return filters.Include(suite); }  

		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;
		public bool IsDryRun;
		public bool XmlConsole;
		public Maybe<string> XmlOutput;
		public bool TeamCityOutput;
		public bool Multicore;
		public bool ShowTimings;
		public string ConfigPath;
		public string[] AssemblyPaths;

		public static ConesoleConfiguration Parse(params string[] args) {
			var result = new ConesoleConfiguration();
			var paths = new List<string>();
			foreach(var item in args) {
				if(!IsOption(item)) 
					paths.Add(Path.GetFullPath(item));
				else result.ParseOption(item);
			}

			result.AssemblyPaths = paths.ToArray();
			return result;
		}

		public static bool IsOption(string value) {
			return value.StartsWith(OptionPrefix);
		}

		void ParseOption(string item) {
			if(item == "--labels") {
				Verbosity = LoggerVerbosity.Labels;
				return;
			}
 
			if(item == "--timings") {
				Verbosity = LoggerVerbosity.Labels;
				ShowTimings = true;
				return;
			}

			if(item == "--test-names") {
				Verbosity = LoggerVerbosity.TestNames;
				return;
			}
			
			if(item == "--dry-run") {
				IsDryRun = true;
				return;
			}

			if(item == "--xml-console") {
				XmlConsole = true;
				return;
			}

			if(item == "--teamcity") {
				TeamCityOutput  = true;
				return;
			}

			if(item == "--multicore") {
				Multicore = true;
				return;
			}

			if(item == "--autotest")
				return;

			if(item == "--debug")
				return;
			
			var m = OptionPattern.Match(item);
			if(!m.Success)
				throw new ArgumentException("Unknown option:" + item);

			var option = m.Groups["option"].Value;
			var valueRaw =  m.Groups["value"].Value;
			if(option == "include-tests") {
				filters.IncludeTests(valueRaw);
			}
			else if(option == "categories") {
				filters.Categories(valueRaw);
			}
			else if(option == "xml") {
				XmlOutput = valueRaw.ToMaybe();
			} else if(option == "config") {
				var fullPath = Path.GetFullPath(valueRaw);
				ConfigPath = fullPath;
			} else 
				throw new ArgumentException("Unknown option:" + item);
		}
	}

	class Program : MarshalByRefObject
	{
		public string[] AssemblyPaths;
		public string[] Options;

		static int Main(string[] args) {
			if(args.Length == 0)
				return DisplayUsage();

			string configPath = null;
			string[] assemblyPaths;
			try {
				var config = ConesoleConfiguration.Parse(args);
				configPath = config.ConfigPath;
				assemblyPaths = config.AssemblyPaths;
			} catch {
				return DisplayUsage();
			}

			if(args.Contains("--debug"))
				System.Diagnostics.Debugger.Launch();

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

		private static int RunTests(string[] args, string[] assemblyPaths, string configPath)
		{
			return CrossDomainConeRunner.WithProxyInDomain<Program, int>(
				configPath,
				assemblyPaths,
				program =>
				{
					program.AssemblyPaths = assemblyPaths;
					program.Options = args;
					return program.Execute();
				});
		}

		int Execute(){
			var result = new ConesoleResultLogger();
			try {
				var config = ConesoleConfiguration.Parse(Options);
				var results = CreateTestSession(config, result);

				if(config.IsDryRun) {
					results.GetTestExecutor = _ => new DryRunTestExecutor();
				}

				new SimpleConeRunner {
					Workers = config.Multicore ? Environment.ProcessorCount : 1,
				}.RunTests(results, CrossDomainConeRunner.LoadTestAssemblies(AssemblyPaths, Error));

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

			return result.FailureCount;
		}

		void Error(string message) { Console.Error.WriteLine(message); }
		void Error(string format, params object[] args) { Console.Error.WriteLine(format, args); }
		void Error(Exception e) { Console.Error.WriteLine(e); }

		static TestSession CreateTestSession(ConesoleConfiguration config, ISessionLogger baseLogger) {
			return new TestSession(CreateLogger(config, baseLogger)) {
				IncludeSuite = config.IncludeSuite,
				ShouldSkipTest = x => !config.IncludeTest(x)
			};
		}

		static ISessionLogger CreateLogger(ConesoleConfiguration config, ISessionLogger baseLogger) {
			var loggers = new List<ISessionLogger> {
				baseLogger
			};

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

			if (config.XmlOutput.IsSomething) {
				loggers.Add(CreateXmlLogger(config.XmlOutput.Value));
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

	class ConesoleResultLogger : ISessionLogger, ISuiteLogger, ITestLogger
	{
		public int FailureCount;

		public void WriteInfo(Action<ISessionWriter> output) { }

		public void BeginSession() { }

		public ISuiteLogger BeginSuite(IConeSuite suite) { return this; }

		public void EndSession() { }

		public ITestLogger BeginTest(IConeTest test) { return this; }

		public void EndSuite() { }

		public void Failure(ConeTestFailure failure) {
			Interlocked.Increment(ref FailureCount);
		}

		public void Success() { }

		public void Pending(string reason) { }

		public void Skipped() { }

		void ITestLogger.BeginTest() { }

		public void EndTest() { }
	}
}
