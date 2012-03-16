using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Runners;

namespace Conesole
{
    class ConesoleConfiguration
    {
        static ConesoleConfiguration ParseCommandlineArgs(string[] args) {
            return new ConesoleConfiguration();
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

            var logger = new ConsoleLogger();
            try {
                new SimpleConeRunner().RunTests(logger, args.Select(Assembly.LoadFrom));
            } catch(ReflectionTypeLoadException tle) {
                foreach(var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
            } catch(Exception e) {
                Console.Error.WriteLine(e);
                return -1;
            }
            return 0;
        }
    }
}
