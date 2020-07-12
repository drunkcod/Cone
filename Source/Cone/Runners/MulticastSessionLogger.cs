using System;
using System.Collections.Generic;
using System.Linq;
using CheckThat.Internals;
using Cone.Core;

namespace Cone.Runners
{
	public class MulticastSessionLogger : ISessionLogger
	{
		class MultiCastSuiteLogger : ISuiteLogger
		{
			readonly ISuiteLogger[] children;

			public MultiCastSuiteLogger(ISuiteLogger[] children) {
				this.children = children;
			}

			public ITestLogger BeginTest(IConeTest test) =>
				new MulticastLogger(children.ConvertAll(x => x.BeginTest(test)));

			public void EndSuite() {
				children.ForEach(x => x.EndSuite());
			}
		}

		class MulticastLogger : ITestLogger
		{
			readonly ITestLogger[] children;
			
			public MulticastLogger(ITestLogger[] children) {
				this.children = children;	
			}

			public void Failure(ConeTestFailure failure) {
				EachChild(x => x.Failure(failure));
			}

			public void Success() {
				EachChild(x => x.Success());
			}

			public void Pending(string reason) {
				EachChild(x => x.Pending(reason));
			}

			public void Skipped() {
				EachChild(x => x.Skipped());
			}

			public void TestStarted() {
				EachChild(x => x.TestStarted());
			}

			public void TestFinished() {
				EachChild(x => x.TestFinished());
			}

			void EachChild(Action<ITestLogger> action) {
				children.ForEach(action);
			}
		}

		readonly ISessionLogger[] children;

		public MulticastSessionLogger(params ISessionLogger[] sessionLoggers) {
			this.children = sessionLoggers;
		}

		public MulticastSessionLogger(IEnumerable<ISessionLogger> sessionLoggers) : this(sessionLoggers.ToArray()) { }

		public void BeginSession() => children.ForEach(x => x.BeginSession());

		public ISuiteLogger BeginSuite(IConeSuite suite) =>
			new MultiCastSuiteLogger(children.ConvertAll(x => x.BeginSuite(suite)));

		public void EndSession() => children.ForEach(x => x.EndSession());

		public void WriteInfo(Action<ISessionWriter> output) =>
			children.ForEach(x => x.WriteInfo(output));
	}
}
