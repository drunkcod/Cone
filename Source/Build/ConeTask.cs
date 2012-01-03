using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace Cone.Build
{
    [Serializable]
    public class RunnerEventArgs : EventArgs
    {
        public string File;
        public int Line;
        public int Column;
        public string Message;
    }

    public interface ICrossDomainLogger 
    {
        void Info(RunnerEventArgs e);
        void Failure(RunnerEventArgs e);
    }
 
    public class CrossDomainConeRunner : MarshalByRefObject
    {
        class CrossDomainLoggerAdapater : IConeLogger
        {
            readonly ICrossDomainLogger crossDomainLog;
            
            public CrossDomainLoggerAdapater(ICrossDomainLogger crossDomainLog) {
                this.crossDomainLog = crossDomainLog;
            }

            void IConeLogger.Info(string format, params object[] args) {
                crossDomainLog.Info(new RunnerEventArgs {
                    Message = string.Format(format, args)
                });
            }

            void IConeLogger.Failure(ConeTestFailure failure) {
                crossDomainLog.Failure(new RunnerEventArgs {
                    File = failure.File,
                    Line = failure.Line,
                    Column = failure.Column,
                    Message = failure.Message
                });
            }
        }

        public void RunTests(ICrossDomainLogger logger, IEnumerable<string> assemblyPaths) {
            new ConePad.SimpleConeRunner() {
                ShowProgress = false
            }.RunTests(new CrossDomainLoggerAdapater(logger), assemblyPaths.Select(x => Assembly.LoadFrom(x)));
        }
    }

    public class ConeTask : MarshalByRefObject, ITask, ICrossDomainLogger
    {
        const string SenderName = "Cone";
        bool noFailures;

        public IBuildEngine BuildEngine { get; set; }

        public bool Execute() {
            noFailures = true;
            var testDomain = AppDomain.CreateDomain("TestDomain", null, new AppDomainSetup {
                ApplicationBase = System.IO.Path.GetDirectoryName(Path),
                ShadowCopyFiles = "true"
            });
            var runner = (CrossDomainConeRunner)testDomain.CreateInstanceAndUnwrap(typeof(CrossDomainConeRunner).Assembly.FullName, typeof(CrossDomainConeRunner).FullName);
            
            runner.RunTests(this, new[]{ Path });
            AppDomain.Unload(testDomain);
            return noFailures;
        }

        void ICrossDomainLogger.Info(RunnerEventArgs e) {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(e.Message, string.Empty, SenderName, MessageImportance.High));
        }

        void ICrossDomainLogger.Failure(RunnerEventArgs e) {
            noFailures = false;
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, e.File, e.Line, 0, 0, e.Column, e.Message, string.Empty, SenderName));
        }

        public ITaskHost HostObject { get; set; }

        [Required]
        public string Path { get; set; }
    }
}
