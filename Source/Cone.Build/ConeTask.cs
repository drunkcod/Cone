using System;
using Cone.Core;
using Cone.Runners;
using Microsoft.Build.Framework;

namespace Cone.Build
{
	public class ConeTask : MarshalByRefObject, ITask, ICrossDomainLogger
	{
		const string SenderName = "Cone";
		bool noFailures;

		public IBuildEngine BuildEngine { get; set; }

		public bool Execute() {
			try {
				noFailures = true;
				CrossDomainConeRunner.RunTestsInTemporaryDomain(this, new CorssDomainRunnerConfiguration {
					ConfigurationPath = ConfigPath, 
					AssemblyPaths = Array.ConvertAll(Path, System.IO.Path.GetFullPath),
					UseMulticore = RunInParallel
				});
				return noFailures;
			} catch(Exception e) {
				BuildEngine.LogErrorEvent(new BuildErrorEventArgs("RuntimeFailure", string.Empty, string.Empty, 0, 0, 0, 0, string.Format("{0}", e), string.Empty, SenderName));
				return false;
			}
		}

		void ICrossDomainLogger.Info(string message) {
			BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, SenderName, MessageImportance.Low));
		}
		void ICrossDomainLogger.Error(string message) {
			BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, SenderName, MessageImportance.High));
		}

		void ICrossDomainLogger.BeginTest(ConeTestName test) { }

		void ICrossDomainLogger.Success() { }

		void ICrossDomainLogger.Failure(string file, int line, int column, string message, string stackTrace) {
			noFailures = false;
			BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, file, line, 0, 0, column, message, string.Empty, SenderName));
		}

		void ICrossDomainLogger.Pending(string reason) { }

		public ITaskHost HostObject { get; set; }

		[Required]
		public string[] Path { get; set; }

		public string ConfigPath { get; set; }

		public bool RunInParallel { get; set; }
	}
}
