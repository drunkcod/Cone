using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public class InProcRunner
	{
		public int Main(string[] args) {
			var results = CreateTestSession();
			var runner = new SimpleConeRunner(new ConeTestNamer());

			var specPath = Path.GetFullPath(args[0]);
			var specBinPath = Path.GetDirectoryName(specPath);
			AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
				var probeName = e.Name.Split(',').First();
				foreach(var ext in new[] { ".dll", ".exe "}) {
					var probeBin = Path.Combine(specBinPath, probeName + ext);
					if(File.Exists(probeBin))
						return Assembly.LoadFrom(probeBin);
				}
				return null;
			};
			var assemblies = new[] { Assembly.LoadFrom(specPath) };
				runner.RunTests(results, assemblies);
			results.Report();
			return results.FailureCount;
		}

		static TestSession CreateTestSession() =>
			new TestSession(new ConsoleSessionLogger(new ConsoleLoggerSettings()));
	}
}
