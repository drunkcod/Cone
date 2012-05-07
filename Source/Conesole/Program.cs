using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Runners;

namespace Conesole
{
    class ConesoleConfiguration
    {
        public IEnumerable<string> AssemblyPaths;

        public static ConesoleConfiguration ParseCommandlineArgs(string[] args) {
            return new ConesoleConfiguration {
                AssemblyPaths = args
            };
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
			var config = ConesoleConfiguration.ParseCommandlineArgs(args);

            try {
				var results = new TestSession(new ConsoleLogger());
				results.ShouldSkipFixture = x => x.Categories.Contains("Acceptance"); 
				results.ShouldSkipTest = x => x.Categories.Contains("Acceptance"); 
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
