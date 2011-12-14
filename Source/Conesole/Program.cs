using System;
using System.Linq;
using System.Reflection;
using Cone;

namespace Conesole
{
    class Program
    {
        class ConsoleRunner : ConePad.SimpleConeRunner
        {
        }

        static void Main(string[] args) {
            try {
                new ConsoleRunner().RunTests(Console.Out, args.Select(x => Assembly.LoadFrom(x)));
            } catch(ReflectionTypeLoadException tle) {
                foreach(var item in tle.LoaderExceptions)
                    Console.Error.WriteLine("{0}\n---", item);
            } catch(Exception e) {
                Console.Error.WriteLine(e);
            }
        }
    }
}
