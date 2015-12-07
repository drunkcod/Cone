using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Cone.Core;
using System.Diagnostics;

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

		public void WriteInfo(Action<ISessionWriter> output) {
			var result = new StringWriter();
			output(new TextSessionWriter(result));
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

		void ITestLogger.BeginTest() { }

		void ITestLogger.EndTest() { }
	}

	public class CorssDomainRunnerConfiguration
	{
		public string ConfigurationPath;
		public string[] AssemblyPaths;
		public bool UseMulticore;
	}

	public class CrossDomainConeRunner
	{
		[Serializable]
		class RunTestsCommand
		{
			public ICrossDomainLogger Logger;
			public string[] AssemblyPaths;
			public bool UseMulticore;

			public void Execute() {
				var logger = new CrossDomainSessionLoggerAdapter(Logger) {
					ShowProgress = false
				};
				new SimpleConeRunner() {
					Workers = UseMulticore ? Environment.ProcessorCount : 1,
				}.RunTests(new TestSession(logger), LoadTestAssemblies(AssemblyPaths, error => Logger.Info(error)));             
			}
		}

		static T WithTestDomain<T>(string configPath, string[] assemblyPaths, Func<AppDomain,T> @do) {
			var domainSetup = new AppDomainSetup {
				ApplicationBase = Path.GetDirectoryName(assemblyPaths.First()),
				ShadowCopyFiles = "False",
			};

			if(string.IsNullOrEmpty(configPath) && assemblyPaths.Length == 1)
				configPath = Path.GetFullPath(assemblyPaths[0] + ".config");
						
			if(File.Exists(configPath))
				domainSetup.ConfigurationFile = configPath;
			
			var conePath = 	new Uri(typeof(CrossDomainConeRunner).Assembly.CodeBase).LocalPath;			
			var localCone = Path.Combine(domainSetup.ApplicationBase, Path.GetFileName(conePath));

			var testDomain = AppDomain.CreateDomain("Cone.TestDomain", 
				AppDomain.CurrentDomain.Evidence,
				domainSetup, 
				new PermissionSet(PermissionState.Unrestricted));
			testDomain.SetData(ConeResolver.ResolvePathsKey, assemblyPaths.ConvertAll(x => Path.GetDirectoryName(x)));
			testDomain.CreateInstanceFrom(
				File.Exists(localCone) 
					? localCone 
					: conePath, 
				typeof(ConeResolver).FullName);

			Environment.CurrentDirectory = domainSetup.ApplicationBase;

			try {
				return @do(testDomain);
			} finally {
				AppDomain.Unload(testDomain);
			}
		}

		public static TResult WithProxyInDomain<T,TResult>(string configPath, string[] assemblyPaths, Func<T, TResult> @do) {
			return WithTestDomain(configPath, assemblyPaths, testDomain => {
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
					testAssemblies.Add(Assembly.LoadFrom(Path.GetFullPath(assemblyPaths[i])));
				}
				catch (FileNotFoundException) {
					logError("Failed to load: " + assemblyPaths[i]);
				}
			return testAssemblies;
		}

		public static void RunTestsInTemporaryDomain(ICrossDomainLogger logger, CorssDomainRunnerConfiguration config) {
			WithTestDomain(config.ConfigurationPath, config.AssemblyPaths, testDomin => {
				var runTests = new RunTestsCommand
				{
					Logger = logger,
					AssemblyPaths = config.AssemblyPaths,
					UseMulticore = config.UseMulticore,
				};
				testDomin.DoCallBack(runTests.Execute);
				return 0;
			});
		}
	}
}
