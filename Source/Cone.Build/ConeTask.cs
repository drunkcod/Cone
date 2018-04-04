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

		void ICrossDomainLogger.Write(LogSeverity severity, string message) =>
			BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, SenderName, ToImportance(severity)));

		static MessageImportance ToImportance(LogSeverity severity) {
			if(severity >= LogSeverity.Error)
				return MessageImportance.High;
			if(severity < LogSeverity.Notice)
				return MessageImportance.Low;
			return MessageImportance.Normal;
		}

		void ICrossDomainLogger.BeginTest(ConeTestName testCase) { }

		void ICrossDomainLogger.Success(ConeTestName testCase) { }

		void ICrossDomainLogger.Failure(ConeTestName testCase, string file, int line, int column, string message, string stackTrace) {
			noFailures = false;
			BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Test ", string.Empty, file, line, 0, 0, column, message, string.Empty, SenderName));
		}

		void ICrossDomainLogger.Pending(ConeTestName testCase, string reason) { }

		public ITaskHost HostObject { get; set; }

		[Required]
		public string[] Path { get; set; }

		public string ConfigPath { get; set; }

		public bool RunInParallel { get; set; }
	}
}
