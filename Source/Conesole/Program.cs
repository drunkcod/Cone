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

namespace Conesole
{
	public class ConesoleConfiguration
    {
		const string OptionPrefix = "--";
		readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix));

		readonly HashSet<string> includedCategories = new HashSet<string>();
		readonly HashSet<string> excludedCategories = new HashSet<string>();

		Predicate<IConeEntity> categoryFilter = _=> true;
		Predicate<IConeTest> testFilter;
		Predicate<IConeSuite> suiteFilter;

		public bool IncludeTest(IConeTest test) { return CategoryCheck(test) && testFilter(test); }
		public bool IncludeSuite(IConeSuite suite) { return CategoryCheck(suite) && suiteFilter(suite); }  

		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;
		public bool IsDryRun;
		public bool XmlConsole;
		public Maybe<string> XmlOutput;
		public bool TeamCityOutput;
        public bool Multicore;

        public static ConesoleConfiguration Parse(params string[] args) {
			var result = new ConesoleConfiguration();
			foreach(var item in args)
        		result.ParseOption(item);
			if(result.testFilter == null)
				result.testFilter = _ => true;
			if(result.suiteFilter == null)
				result.suiteFilter = _ => true;
        	return result;
        }

		public static bool IsOption(string value) {
			return value.StartsWith(OptionPrefix);
		}

		bool CategoryCheck(IConeEntity entity) {
			return (includedCategories.IsEmpty() || entity.Categories.Any(includedCategories.Contains)) 
				&& !entity.Categories.Any(excludedCategories.Contains);
		}

		void ParseOption(string item) {
			if(!IsOption(item))
				return;

			if(item == "--labels") {
				Verbosity = LoggerVerbosity.Labels;
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

            if (item == "--multicore") {
                Multicore = true;
                return;
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

				suiteFilter = suiteFilter.Or(x => suitePatternRegex.IsMatch(x.Name));
				testFilter = testFilter.Or(x => testPatternRegex.IsMatch(x.Name));
			}
			else if(option == "categories") {
				foreach(var category in valueRaw.Split(','))
					if(category.StartsWith("!"))
						excludedCategories.Add(category.Substring(1));
					else 
						includedCategories.Add(category);
			}
			else if(option == "xml") {
				XmlOutput = valueRaw.ToMaybe();
			} else 
				throw new ArgumentException("Unknown option:" + item);
		}
		static Regex CreatePatternRegex(string pattern) {
			return new Regex("^" + pattern
				.Replace("\\", "\\\\")
				.Replace(".", "\\.")
				.Replace("*", ".*?"));
		}
    }

    class Program : MarshalByRefObject
    {
		public string[] AssemblyPaths;
		public string[] Options;

		static int Main(string[] args) {
            if(args.Length == 0)
            	return DisplayUsage();

			var assemblyPaths = args
				.Where(x => !ConesoleConfiguration.IsOption(x))
				.Select(Path.GetFullPath)
				.ToArray();

			return CrossDomainConeRunner.WithProxyInDomain<Program, int>(
				Path.GetDirectoryName(Path.GetFullPath(assemblyPaths.FirstOrDefault() ?? ".")), 
				assemblyPaths, 
				runner => {
					runner.AssemblyPaths = assemblyPaths;
					runner.Options = args;
					return runner.Execute();
			});
        }

		int Execute(){
            try {
				var config = ConesoleConfiguration.Parse(Options);
            	var results = CreateTestSession(config);

				if(config.IsDryRun) {
					results.GetTestExecutor = _ => new DryRunTestExecutor();
				}

                new SimpleConeRunner {
                    Workers = config.Multicore ? Environment.ProcessorCount : 1,
                }.RunTests(results, CrossDomainConeRunner.LoadTestAssemblies(AssemblyPaths));

            } catch (ReflectionTypeLoadException tle) {
                foreach (var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
				return -1;
			} catch(ArgumentException e) {
				Console.Error.WriteLine(e.Message);
				return DisplayUsage();
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return -1;
            }

			return 0;
		}

        static TestSession CreateTestSession(ConesoleConfiguration config) {
            return new TestSession(CreateLogger(config)) {
                IncludeSuite = config.IncludeSuite,
                ShouldSkipTest = x => !config.IncludeTest(x)
            };;
		}

		static ISessionLogger CreateLogger(ConesoleConfiguration config) {
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
				};
				loggers.Add(new ConsoleSessionLogger(settings));
			}

			if (config.XmlOutput.IsSomething) {
                loggers.Add(new XmlSessionLogger(new XmlTextWriter(config.XmlOutput.Value, Encoding.UTF8){
                    Formatting = Formatting.Indented
                }));
			} 
			return loggers.Count == 1
				? loggers[0]
				: new MulticastSessionLogger(loggers);
		}

    	static int DisplayUsage() {
    		using(var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Conesole.Usage.txt"))) {
    			Console.WriteLine(reader.ReadToEnd());
    		}
    		return -1;
    	}
    }
}
