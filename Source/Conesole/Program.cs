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
    public class ConesoleConfiguration
    {
        public IEnumerable<string> AssemblyPaths;
		public Predicate<IConeTest> IncludeTest = _ => true;
		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;

        public static ConesoleConfiguration Parse(params string[] args) {
			var argPattern = new Regex("^--(?<option>.+)=(?<value>.+$)");
			var paths = new List<string>();
			var result = new ConesoleConfiguration { AssemblyPaths = paths };
			foreach(var item in args) {
				if(item == "--labels") {
					result.Verbosity = LoggerVerbosity.TestName;
					continue;
				}
				var m = argPattern.Match(item);
				if(!m.Success)
					paths.Add(item);
				else {
					var option = m.Groups["option"].Value;
					var valueRaw =  m.Groups["value"].Value;
					if(option == "include-tests") {
						if(!valueRaw.Contains("."))
							valueRaw = "*." + valueRaw;
					
						var value = "^" + valueRaw
							.Replace("\\", "\\\\")
							.Replace(".", "\\.")
							.Replace("*", ".*?");

						result.IncludeTest = x => Regex.IsMatch(x.Name.FullName, value);
					} else {
						throw new ArgumentException("Unknown option:" + option);
					}
				}
			}
			return result;
        }
    }

    class Program
    {
        static int Main(string[] args) {
            if(args.Length == 0) {
                using(var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Conesole.Usage.txt"))) {
                    Console.WriteLine(reader.ReadToEnd());
                }
                return -1;
            }
			var config = ConesoleConfiguration.Parse(args);

            try {
            	var results = new TestSession(new ConsoleLogger { Verbosity = config.Verbosity });
				results.ShouldSkipTest = x => !config.IncludeTest(x);
            	new SimpleConeRunner().RunTests(results, LoadTestAssemblies(config));
            } catch (ReflectionTypeLoadException tle) {
                foreach (var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return -1;
            }
            return 0;
        }

    	static IEnumerable<Assembly> LoadTestAssemblies(ConesoleConfiguration config) {
    		return config.AssemblyPaths.Select(Assembly.LoadFrom);
    	}
    }
}
