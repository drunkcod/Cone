using System;
using Cone.Runners;
using Microsoft.Build.Framework;

namespace Cone.Build
{
    [Serializable]
    public class CrossDomainRunTestsCommand
    {
        public ICrossDomainLogger Logger;
        public string[] Paths;

        public void Execute() {
            new CrossDomainConeRunner().RunTests(Logger, Paths);
        }
    }

    public class ConeTask : MarshalByRefObject, ITask, ICrossDomainLogger
    {
        const string SenderName = "Cone";
        bool noFailures;

        public IBuildEngine BuildEngine { get; set; }

        public bool Execute() {
            try {
                noFailures = true;
                var testDomain = AppDomain.CreateDomain("TestDomain", null, new AppDomainSetup {
                    ApplicationBase = System.IO.Path.GetDirectoryName(Path),
                    ShadowCopyFiles = "true"
                });

                testDomain.DoCallBack(new CrossDomainRunTestsCommand { Logger = this, Paths = new[] { Path } }.Execute);
                AppDomain.Unload(testDomain);
                return noFailures;
            } catch(Exception e) {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("RuntimeFailure", string.Empty, string.Empty, 0, 0, 0, 0, string.Format("{0}", e), string.Empty, SenderName));
                return false;
            }
        }

        void ICrossDomainLogger.Info(string message) {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, SenderName, MessageImportance.High));
        }

        void ICrossDomainLogger.Failure(string file, int line, int column, string message) {
            noFailures = false;
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, file, line, 0, 0, column, message, string.Empty, SenderName));
        }

        public ITaskHost HostObject { get; set; }

        [Required]
        public string Path { get; set; }
    }
}
