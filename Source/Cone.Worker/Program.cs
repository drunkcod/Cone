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

			var cone = Assembly.LoadFrom(GetConePath(workingDir));
			var inProcRunnerType = cone.GetType("Cone.Runners.ConesoleRunner");
			Console.OutputEncoding = Encoding.UTF8;
			AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
				//Console.WriteLine("Failed to load:" + e.Name);
				return null;
			};
			return (int)inProcRunnerType.GetMethod("Main").Invoke(null, new[] { args });
        }

		static string GetConePath(string workingDir) => Path.Combine(workingDir, "Cone.dll");

		static bool RanInTestDomain(string target, string workingDir, string[] args, out int result) {
#if NET45 || NET462
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
#elif NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2
			result = 0;
			return false;
#endif
		}
	}
}
