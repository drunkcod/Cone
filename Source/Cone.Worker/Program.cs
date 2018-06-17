using System;
using System.IO;
using System.Reflection;

namespace Cone.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
			var target = Path.GetFullPath(args[0]);
			var workingDir = Path.GetDirectoryName(target);
			var cone = Assembly.LoadFrom(Path.Combine(workingDir, "Cone.dll"));
			var inProcRunnerType = cone.GetType("Cone.Runners.ConesoleRunner");
			inProcRunnerType.GetMethod("Main").Invoke(null, new[] { args });
        }
    }
}
