using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            public ITestLogger BeginTest(IConeTest test) {
                var log = new MulticastLogger();
                children.ForEach(x => log.Add(x.BeginTest(test)));
                return log;
            }

            public void EndSuite() {
                children.ForEach(x => x.EndSuite());
            }
        }

        class MulticastLogger : ITestLogger
        {
            readonly List<ITestLogger> children = new List<ITestLogger>();

            public void Add(ITestLogger log) {
                children.Add(log);
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

			public void EndTest() {
				EachChild(x => x.EndTest());
			}

			void EachChild(Action<ITestLogger> action) {
				children.ForEach(action);
			}
        }

        readonly List<ISessionLogger> children = new List<ISessionLogger>();

        public MulticastSessionLogger(params ISessionLogger[] sessionLoggers) : this(sessionLoggers.AsEnumerable()) { }

        public MulticastSessionLogger(IEnumerable<ISessionLogger> sessionLoggers) {
            this.children.AddRange(sessionLoggers);
        }

		public void Add(ISessionLogger log) {
            children.Add(log);
        }

        public void BeginSession() {
            children.ForEach(x => x.BeginSession());
        }

        public ISuiteLogger BeginSuite(IConeSuite suite) {
            return new MultiCastSuiteLogger(children.ConvertAll(x => x.BeginSuite(suite)).ToArray());
        }

        public void EndSession() {
            children.ForEach(x => x.EndSession());
        }

        public void WriteInfo(Action<TextWriter> output) {
            using (var outputResult = new StringWriter()) {
                output(outputResult);
                var result = outputResult.ToString();
                children.ForEach(x => x.WriteInfo(writer => writer.Write(result)));
            }
        }
    }
}
