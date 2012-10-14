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
 
    class CrossDomainLoggerAdapater : IConeLogger
    {
        readonly ICrossDomainLogger crossDomainLog;
            
        public bool ShowProgress { get; set; }

        public CrossDomainLoggerAdapater(ICrossDomainLogger crossDomainLog) {
            this.crossDomainLog = crossDomainLog;
        }

		void IConeLogger.BeginSession() { }
		void IConeLogger.EndSession() { }

        void IConeLogger.WriteInfo(Action<TextWriter> output) {
            var result = new StringWriter();
            output(result);
            crossDomainLog.Info(result.ToString());
        }

        void IConeLogger.Failure(ConeTestFailure failure) {
            crossDomainLog.Failure(
                failure.File,
                failure.Line,
                failure.Column,
                failure.Message);
        }

        void IConeLogger.Success(IConeTest test) {
            if(ShowProgress)
                crossDomainLog.Info(".");
        }

        void IConeLogger.Pending(IConeTest test) {
            if(ShowProgress)
                crossDomainLog.Info("?");
        }

        void IConeLogger.Skipped(IConeTest test) { }
    }

    public class CrossDomainConeRunner
    {
		[Serializable]
        class RunTestsCommand
        {
            public ICrossDomainLogger Logger;
            public string[] AssemblyPaths;

            public void Execute() {
				var log = new CrossDomainLoggerAdapater(Logger) {
                    ShowProgress = false
                };
                new SimpleConeRunner().RunTests(new TestSession(log), LoadTestAssemblies(AssemblyPaths));             
            }
        }

		static T WithTestDomain<T>(string applicationBase, string[] assemblyPaths, Func<AppDomain,T> @do) {
			var domainSetup = new AppDomainSetup {
				ApplicationBase = applicationBase,
				ShadowCopyFiles = "True",
			};
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

		public static IEnumerable<Assembly> LoadTestAssemblies(string[] assemblyPaths) {
			if(assemblyPaths.IsEmpty())
				throw new ArgumentException("No test assemblies specified");
			return assemblyPaths.Select(item => Assembly.LoadFile(Path.GetFullPath(item)));
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
