using System;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
    public interface ICrossDomainLogger 
    {
        void Info(string message);
        void Failure(string file, int line, int column, string message);
    }
 
    public class CrossDomainLoggerAdapater : IConeLogger
    {
        readonly ICrossDomainLogger crossDomainLog;
            
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
    }

    public class CrossDomainConeRunner : MarshalByRefObject
    {
        public void RunTests(ICrossDomainLogger logger, string[] assemblyPaths) {
            
             new SimpleConeRunner() {
                ShowProgress = false
            }.RunTests(new CrossDomainLoggerAdapater(logger), assemblyPaths.Select(x => Assembly.LoadFrom(x)));             
        }
    }
}
