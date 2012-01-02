using System.Reflection;
using Microsoft.Build.Framework;
using System;
using System.IO;

namespace Cone.Build
{
    public class ConeTask : ITask, IConeLogger
    {
        bool noFailures = true;
        public IBuildEngine BuildEngine { get; set; }

        public bool Execute() {
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Path);
            new ConePad.SimpleConeRunner().RunTests(this, new[]{ Assembly.LoadFrom(Path) });
            return noFailures;
        }

        public ITaskHost HostObject { get; set; }

        [Required]
        public string Path { get; set; }

        void IConeLogger.Info(string format, params object[] args) {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format(format, args), string.Empty, string.Empty, MessageImportance.Low));
        }

        public void Failure(ConeTestFailure failure) {
            noFailures = false;
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, failure.File, failure.Line, 0, 0, failure.Column, failure.Message, string.Empty, "Cone"));
        }
    }
}
