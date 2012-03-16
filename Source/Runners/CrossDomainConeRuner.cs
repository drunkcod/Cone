using System;
using System.Reflection;
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

        void IConeLogger.Info(string format, params object[] args) {
            crossDomainLog.Info(string.Format(format, args));
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
    }

    public class CrossDomainConeRunner
    {
        [Serializable]
        class RunTestsCommand
        {
            public ICrossDomainLogger Logger;
            public string[] AssemblyPaths;

            public void Execute() {
                new SimpleConeRunner() {
                    ShowProgress = false
                }.RunTests(new CrossDomainLoggerAdapater(Logger) {
                    ShowProgress = false
                }, AssemblyPaths.ConvertAll(Assembly.LoadFrom));             
            }
        }

        public static void RunTestsInTemporaryDomain(ICrossDomainLogger logger, string applicationBase, string[] assemblyPaths) {
            var testDomain = AppDomain.CreateDomain("TestDomain", null, new AppDomainSetup {
                    ApplicationBase = applicationBase,
                    ShadowCopyFiles = "true"
                });
                testDomain.DoCallBack(new RunTestsCommand {
                    Logger = logger,
                    AssemblyPaths = assemblyPaths
                }.Execute);
                AppDomain.Unload(testDomain);
        }
    }
}
