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
		const string OptionPrefix = "--";
		readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix));

        public IEnumerable<string> AssemblyPaths;
		public Predicate<IConeTest> IncludeTest = _ => true;
		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;

        public static ConesoleConfiguration Parse(params string[] args) {
			var paths = new List<string>();
			var result = new ConesoleConfiguration { AssemblyPaths = paths };
        	paths.AddRange(args.Where(item => !result.ParseOption(item)));
        	return result;
        }

		bool ParseOption(string item) {
			if(!item.StartsWith(OptionPrefix))
				return false;

			if(item == "--labels") {
				Verbosity = LoggerVerbosity.TestName;
				return true;
			}

			var m = OptionPattern.Match(item);
			if(!m.Success)
				throw new ArgumentException("Unknown option:" + item);

			var option = m.Groups["option"].Value;
			var valueRaw =  m.Groups["value"].Value;
			if(option == "include-tests") {
				if(!valueRaw.Contains("."))
					valueRaw = "*." + valueRaw;
					
				var value = "^" + valueRaw
					.Replace("\\", "\\\\")
					.Replace(".", "\\.")
					.Replace("*", ".*?");

				IncludeTest = x => Regex.IsMatch(x.Name.FullName, value);
			}
			return true;
		}
    }

    class Program
    {
        static int Main(string[] args) {
            if(args.Length == 0)
            	return DisplayUsage();

            try {
				var config = ConesoleConfiguration.Parse(args);
            	var results = new TestSession(new ConsoleLogger { Verbosity = config.Verbosity });
				results.ShouldSkipTest = x => !config.IncludeTest(x);
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
