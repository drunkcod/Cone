using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Cone.Core;

namespace Cone.Runners
{
	public interface ICrossDomainLogger 
	{
		void Info(string message);
		void Failure(string file, int line, int column, string message);
	}

	class CrossDomainSessionLoggerAdapter : ISessionLogger, ISuiteLogger, ITestLogger
	{
		readonly ICrossDomainLogger crossDomainLog;

		public CrossDomainSessionLoggerAdapter(ICrossDomainLogger crossDomainLog) {
			this.crossDomainLog = crossDomainLog;
		}

		public bool ShowProgress { get; set; }

		public void WriteInfo(Action<TextWriter> output) {
			var result = new StringWriter();
			output(result);
			crossDomainLog.Info(result.ToString());
		}

		public void BeginSession() { }

		public ISuiteLogger BeginSuite(IConeSuite suite) {
			return this;
		}

		public void EndSuite() { }

		public ITestLogger BeginTest(IConeTest test) {
			return this;
		}

		public void EndSession() { }

		void ITestLogger.Failure(ConeTestFailure failure) {
			crossDomainLog.Failure(
				failure.File,
				failure.Line,
				failure.Column,
				failure.Message);
		}

		void ITestLogger.Success() {
			if (ShowProgress)
				crossDomainLog.Info(".");
		}

		void ITestLogger.Pending(string reason) {
			if (ShowProgress)
				crossDomainLog.Info("?");
		}

		void ITestLogger.Skipped() { }

		void ITestLogger.EndTest() { }
	}

	public class CrossDomainConeRunner
	{
		[Serializable]
		class RunTestsCommand
		{
			public ICrossDomainLogger Logger;
			public string[] AssemblyPaths;

			public void Execute() {
				var logger = new CrossDomainSessionLoggerAdapter(Logger) {
					ShowProgress = false
				};
				new SimpleConeRunner().RunTests(new TestSession(logger), LoadTestAssemblies(AssemblyPaths, error => Logger.Info(error)));             
			}
		}

		static T WithTestDomain<T>(string applicationBase, string[] assemblyPaths, Func<AppDomain,T> @do) {
			var domainSetup = new AppDomainSetup {
				ApplicationBase = applicationBase,
				ShadowCopyFiles = "False",
			};
			Environment.CurrentDirectory = applicationBase;
			if(assemblyPaths.Length == 1) {
				var configPath = Path.GetFullPath(assemblyPaths[0] + ".config");
				if(File.Exists(configPath))
					domainSetup.ConfigurationFile = configPath;
			}
			var testDomain = AppDomain.CreateDomain("Cone.TestDomain", 
				null,
				domainSetup, 
				new PermissionSet(PermissionState.Unrestricted));
			try {
				return @do(testDomain);
			} finally {
				AppDomain.Unload(testDomain);
			}
		}

		public static TResult WithProxyInDomain<T,TResult>(string applicationBase, string[] assemblyPaths, Func<T, TResult> @do) {
			return WithTestDomain(applicationBase, assemblyPaths, testDomain => {
				var proxy = (T)testDomain.CreateInstanceFrom(typeof(T).Assembly.Location, typeof(T).FullName).Unwrap();
				return @do(proxy);
			});
		}

		public static IEnumerable<Assembly> LoadTestAssemblies(string[] assemblyPaths, Action<string> logError) {
			if(assemblyPaths.IsEmpty())
				throw new ArgumentException("No test assemblies specified");
			var testAssemblies = new List<Assembly>();
			for(var i = 0; i != assemblyPaths.Length; ++i)
				try { 
					testAssemblies.Add(Assembly.LoadFile(Path.GetFullPath(assemblyPaths[i])));
				}
				catch (FileNotFoundException ex) {
					logError("Failed to load: " + assemblyPaths[i]);
				}
			return testAssemblies;
		}

		public static void RunTestsInTemporaryDomain(ICrossDomainLogger logger, string applicationBase, string[] assemblyPaths) {
			WithTestDomain(applicationBase, assemblyPaths, testDomin => {
				var runTests = new RunTestsCommand
				{
					Logger = logger,
					AssemblyPaths = assemblyPaths
				};
				testDomin.DoCallBack(runTests.Execute);
				return 0;
			});
		}
	}
}
