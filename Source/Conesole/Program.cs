using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cone.Core;
using Cone.Runners;

namespace Conesole
{
	static class PredicateExtensions
	{
		public static Predicate<T> And<T>(this Predicate<T> self, Predicate<T> andAlso) {
			return self == null ? andAlso : x => self(x) && andAlso(x);
		}

		public static Predicate<T> Or<T>(this Predicate<T> self, Predicate<T> orElse) {
			return self ==  null ? orElse : x => self(x) || orElse(x);
		}
	}

    public class ConesoleConfiguration
    {
		const string OptionPrefix = "--";
		readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix));

        public IEnumerable<string> AssemblyPaths;
		public Predicate<IConeTest> IncludeTest;
		public Predicate<IConeSuite> IncludeSuite = _ => true;  

		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;
		public bool IsDryRun;

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

    class Program
    {
        static int Main(string[] args) {
            if(args.Length == 0)
            	return DisplayUsage();

            try {
				var config = ConesoleConfiguration.Parse(args);
				var logger = new ConsoleLogger { Verbosity = config.Verbosity };
            	var results = new TestSession(logger) {
					IncludeSuite = config.IncludeSuite,
					ShouldSkipTest = x => !config.IncludeTest(x)
				};
				if(config.IsDryRun) {
					results.GetResultCollector = _ => (test, result) => result.Success();
					logger.SuccessColor = ConsoleColor.DarkGray;
				}
            	new SimpleConeRunner().RunTests(results, LoadTestAssemblies(config));
            } catch (ReflectionTypeLoadException tle) {
                foreach (var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
			} catch(ArgumentException e) {
				Console.Error.WriteLine(e.Message);
				return DisplayUsage();
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return -1;
            }
            return 0;
        }

    	private static int DisplayUsage()
    	{
    		using(var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Conesole.Usage.txt")))
    		{
    			Console.WriteLine(reader.ReadToEnd());
    		}
    		return -1;
    	}

    	static IEnumerable<Assembly> LoadTestAssemblies(ConesoleConfiguration config) {
    		return config.AssemblyPaths.Select(Assembly.LoadFrom);
    	}
    }
}
