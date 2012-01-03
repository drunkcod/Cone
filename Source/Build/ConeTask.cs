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
 
    public class CrossDomainConeRunner : MarshalByRefObject, IConeLogger
    {
        public EventHandler<RunnerEventArgs> Info;
        public EventHandler<RunnerEventArgs> Failure;

        public void RunTests(IEnumerable<string> assemblyPaths) {
            new ConePad.SimpleConeRunner() {
                ShowProgress = false
            }.RunTests(this, assemblyPaths.Select(x => Assembly.LoadFrom(x)));
        }

        void IConeLogger.Info(string format, params object[] args) {
            Info(this, new RunnerEventArgs {
                Message = string.Format(format, args)
            });
        }

        void IConeLogger.Failure(ConeTestFailure failure) {
            Failure(this, new RunnerEventArgs {
                File = failure.File,
                Line = failure.Line,
                Column = failure.Column,
                Message = failure.Message
            });
        }
    }

    public class ConeTask : MarshalByRefObject, ITask
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
            
            runner.Info += (sender, e) => BuildEngine.LogMessageEvent(new BuildMessageEventArgs(e.Message, string.Empty, SenderName, MessageImportance.High));                     
            runner.Failure += (sender, e) => Failure(sender, e);

            runner.RunTests(new[]{ Path });
            AppDomain.Unload(testDomain);
            return noFailures;
        }

        void Failure(object sender, RunnerEventArgs e) {
            noFailures = false;
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, e.File, e.Line, 0, 0, e.Column, e.Message, string.Empty, SenderName));
        }

        public ITaskHost HostObject { get; set; }

        [Required]
        public string Path { get; set; }
    }
}
