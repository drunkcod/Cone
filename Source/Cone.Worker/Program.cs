using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Cone.Worker
{
	class Program
    {
        static int Main(string[] args)
        {
			var target = Path.GetFullPath(args[0]);
			var workingDir = Path.GetDirectoryName(target);
			if(RanInTestDomain(target, workingDir, args, out var result))
				return result;
			var cone = Assembly.LoadFrom(Path.Combine(workingDir, "Cone.dll"));
			var inProcRunnerType = cone.GetType("Cone.Runners.ConesoleRunner");
			Console.OutputEncoding = Encoding.UTF8;
			return (int)inProcRunnerType.GetMethod("Main").Invoke(null, new[] { args });
        }

		static bool RanInTestDomain(string target, string workingDir, string[] args, out int result) {
#if NET45
			var targetConfig = target + ".config";
			if(!File.Exists(targetConfig) || !AppDomain.CurrentDomain.IsDefaultAppDomain()) {
				result = 0;
				return false;
			}
			var domainSetup = new AppDomainSetup {
				ApplicationBase = workingDir,
				ShadowCopyFiles = "False",
				ConfigurationFile = targetConfig
			};

			var testDomain = AppDomain.CreateDomain("Cone.TestDomain", 
				AppDomain.CurrentDomain.Evidence,
				domainSetup, 
				new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
			result = testDomain.ExecuteAssembly(new Uri(typeof(Program).Assembly.CodeBase).LocalPath, args);
			return true;
#elif NETCOREAPP2_0
			result = 0;
			return false;
#endif
		}
	}
}
