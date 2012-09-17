using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Cone.Core;
using Cone.Runners;

namespace Conesole
{
	public class ConesoleConfiguration
    {
		const string OptionPrefix = "--";
		readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix));

        public ICollection<string> AssemblyPaths;
		public Predicate<IConeTest> IncludeTest;
		public Predicate<IConeSuite> IncludeSuite = _ => true;  

		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;
		public bool IsDryRun;
		public bool XmlOutput;

        public static ConesoleConfiguration Parse(params string[] args) {
			var paths = new List<string>();
			var result = new ConesoleConfiguration { AssemblyPaths = paths };
        	paths.AddRange(args.Where(item => !result.ParseOption(item)));
			if(result.IncludeTest == null)
				result.IncludeTest = _ => true;
        	return result;
        }

		bool ParseOption(string item) {
			if(!item.StartsWith(OptionPrefix))
				return false;

			if(item == "--labels") {
				Verbosity = LoggerVerbosity.TestName;
				return true;
			} 
			
			if(item == "--dry-run") {
				IsDryRun = true;
				return true;
			}

			if(item == "--xml-console") {
				XmlOutput = true;
				return true;
			}

			var m = OptionPattern.Match(item);
			if(!m.Success)
				throw new ArgumentException("Unknown option:" + item);

			var option = m.Groups["option"].Value;
			var valueRaw =  m.Groups["value"].Value;
			if(option == "include-tests") {
				var suitePattern = "*";
				var parts = valueRaw.Split('.');

				if(parts.Length > 1)
					suitePattern = string.Join(".", parts, 0, parts.Length - 1);
					
				var testPatternRegex = CreatePatternRegex(suitePattern + "." + parts.Last());
				var suitePatternRegex = CreatePatternRegex(suitePattern);

				IncludeSuite = IncludeSuite.Or(x => suitePatternRegex.IsMatch(x.Name));
				IncludeTest = IncludeTest.Or(x => testPatternRegex.IsMatch(x.Name.FullName));
			}
			else if(option == "categories") {
				var excluded = new HashSet<string>();
				foreach(var category in valueRaw.Split(','))
					if(category.StartsWith("!"))
						excluded.Add(category.Substring(1));
				IncludeSuite = IncludeSuite.And(x => !x.Categories.Any(excluded.Contains));
				IncludeTest = IncludeTest.And(x => !x.Categories.Any(excluded.Contains));
			}
			else 
				throw new ArgumentException("Unknown option:" + item);

			return true;
		}
		static Regex CreatePatternRegex(string pattern) {
			return new Regex("^" + pattern
				.Replace("\\", "\\\\")
				.Replace(".", "\\.")
				.Replace("*", ".*?"));
		}
    }

	interface IConesoleResult 
	{
		int ExitCode { get; set;}
	}

	class ConesoleResult : MarshalByRefObject, IConesoleResult
	{
		public int ExitCode { get; set; }
	}

	[Serializable]
    class Program
    {
		public IConesoleResult Result = new ConesoleResult();
		public string[] Arguments;

		static int Main(string[] args) {
            if(args.Length == 0)
            	return DisplayUsage();

			var testDomain = AppDomain.CreateDomain("Conesole.TestDomain");
			var runner = new Program {
				Arguments = args,
			};

			testDomain.AssemblyResolve += AssemblyResolve;
			testDomain.DoCallBack(runner.Execute);
			AppDomain.Unload(testDomain);

			return runner.Result.ExitCode;
        }

		static Assembly AssemblyResolve(object sender, ResolveEventArgs e) {
			var baseName = e.Name.Substring(0, e.Name.IndexOf(','));
			var probe = Path.Combine(GetBaseDir(e), baseName) + ".dll";
			if(File.Exists(probe))
				return Assembly.LoadFile(probe);
			return null;
		}

		static string GetBaseDir(ResolveEventArgs e) {
			if(e.RequestingAssembly == null)
				return Environment.CurrentDirectory;
			return Path.GetDirectoryName(new Uri(e.RequestingAssembly.CodeBase).LocalPath);
		}

		void Execute(){
            try {
				var config = ConesoleConfiguration.Parse(Arguments);
				var logger = CreateLogger(config);
            	var results = new TestSession(logger) {
					IncludeSuite = config.IncludeSuite,
					ShouldSkipTest = x => !config.IncludeTest(x)
				};

				if(config.IsDryRun) {
					results.GetResultCollector = _ => (test, result) => result.Success();
				}
            	
				new SimpleConeRunner().RunTests(results, LoadTestAssemblies(config));

	            Result.ExitCode = 0;
            } catch (ReflectionTypeLoadException tle) {
                foreach (var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
			} catch(ArgumentException e) {
				Console.Error.WriteLine(e.Message);
				Result.ExitCode = DisplayUsage();
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                Result.ExitCode = -1;
            }
		}

		static IConeLogger CreateLogger(ConesoleConfiguration config) {
			if(config.XmlOutput)
				return new XmlLogger(new XmlTextWriter(Console.Out) {
					Formatting = Formatting.Indented
				});

			var logger = new ConsoleLogger { Verbosity = config.Verbosity };
			if(config.IsDryRun)
				logger.SuccessColor = ConsoleColor.DarkGreen;
			return logger;
		}

    	static int DisplayUsage() {
    		using(var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Conesole.Usage.txt"))) {
    			Console.WriteLine(reader.ReadToEnd());
    		}
    		return -1;
    	}

    	static IEnumerable<Assembly> LoadTestAssemblies(ConesoleConfiguration config) {
			var paths = config.AssemblyPaths;
			if(paths.IsEmpty())
				throw new ArgumentException("No test assemblies specified");
			return paths.Select(item => Assembly.LoadFile(Path.GetFullPath(item)));
    	}
    }
}
