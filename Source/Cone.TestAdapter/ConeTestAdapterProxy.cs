using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Cone.Core;
using Cone.Runners;
using Newtonsoft.Json;

namespace Cone.TestAdapter
{
	class ConeTestAdapterProxy : MarshalByRefObject
	{
		readonly ConeTestNamer names = new ConeTestNamer();

		public int DiscoverTests(string source, ICrossDomainLogger sink) {
			var session = CreateSession(sink, "DiscoverTests", source);
			session.GetTestExecutor = _ => new DryRunTestExecutor();

			return RunSession(source, true, sink, session);
		}

		public int RunTests(string source, ICrossDomainLogger sink) =>
			RunSession(source, false, sink, CreateSession(sink, "RunAll", source));

        public int RunTests(string source, ICrossDomainLogger sink, IEnumerable<string> tests) {
			var testsToRun = new HashSet<string>(tests);
			var session = CreateSession(sink, "RunSelected", source);
			session.ShouldSkipTest = test => !testsToRun.Contains(test.Name);

			return RunSession(source, false, sink, session);
		}

		private int RunSession(string source, bool multiCore, ICrossDomainLogger sink, TestSession session) {			
			var loaded = CrossDomainConeRunner.LoadTestAssemblies(new [] {source}, sink.Error);
			Check.IncludeGuide = false;
			var runner = SimpleConeRunner.ConeOnlyRunner(names);
			runner.Workers = multiCore ? Environment.ProcessorCount : 1;
			runner.RunTests(session, loaded);
			return 0;
		}

		TestSession CreateSession(ICrossDomainLogger sink, string context, string source) {
			ISessionLogger log = new CrossDomainSessionLoggerAdapter(sink);
			if(TryConnectDebugTap(context, source, out var debugTap))
				log = new MulticastSessionLogger(log, debugTap);
			return new TestSession(log);
		}

		bool TryConnectDebugTap(string context, string source, out DebugTapLogger debugTap) {
			try {
				var pipe = new NamedPipeClientStream("Cone.DebugTap+json");
				pipe.Connect(0);
				debugTap = DebugTapLogger.Create(pipe, context, source);
				return true;
			} catch { }
			debugTap = null;
			return false;
		}

		class DebugTapLogger : ISessionLogger
		{
			class DebugTapSuiteLogger : ISuiteLogger
			{
				public ITestLogger BeginTest(IConeTest test) {
					return new DebugTapTestLogger();
				}

				public void EndSuite() {
				}
			}

			class DebugTapTestLogger : ITestLogger
			{
				public void Failure(ConeTestFailure failure) {
				}

				public void Pending(string reason) {
				}

				public void Skipped() {
				}

				public void Success() {
				}

				public void TestFinished() {
				}

				public void TestStarted() {
				}
			}

			readonly Stream output;
			readonly int pid;
			readonly string context;
			readonly string source;
			readonly Encoding encoding = Encoding.UTF8;

			DebugTapLogger(Stream output, int pid, string context, string source) {
				this.output = output;
				this.pid = pid;
				this.context = context;
				this.source = source;
			}

			public static DebugTapLogger Create(Stream output, string context, string source) {
				return new DebugTapLogger(output, Process.GetCurrentProcess().Id, context, source);
			}

			public void BeginSession() {
				Write(new { beginSession = new { pid, context, source }});
			}

			public ISuiteLogger BeginSuite(IConeSuite suite) {
				Write(new { beginSuite = new { pid, name = suite.Name } });
				return new DebugTapSuiteLogger();
			}

			public void EndSession() {
				Write(new { endSession = new { pid } });
			}

			public void WriteInfo(Action<ISessionWriter> output) {
			}

			void Write(object obj) {
				try { 
					var s = new StringWriter();
					s.WriteLine(JsonConvert.SerializeObject(obj));
					var b = encoding.GetBytes(s.ToString());
					output.Write(b, 0, b.Length);
				} catch { }
			}
		}
	}
}