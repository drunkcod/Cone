using System.Collections.Generic;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
{
	public interface IMethodClassifier
	{
		void Classify(MethodInfo method);
	}

	public abstract class MethodClassifier : IMethodClassifier
	{
		readonly IConeFixtureMethodSink fixtureSink;
		readonly IConeTestMethodSink testSink;

		public MethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) {
			this.fixtureSink = fixtureSink;
			this.testSink = testSink;
		}

		public void Classify(MethodInfo method) {
			if(method.DeclaringType == typeof(object)) {
				Unintresting(method);
				return;
			}
			ClassifyCore(method);
		}

		protected abstract void ClassifyCore(MethodInfo method);

		protected void BeforeAll(MethodInfo method) { fixtureSink.BeforeAll(method); }
		protected void BeforeEach(MethodInfo method) { fixtureSink.BeforeEach(method); }
		protected void AfterEach(MethodInfo method) { fixtureSink.AfterEach(method); }
		protected void AfterEachWithResult(MethodInfo method) { fixtureSink.AfterEachWithResult(method); }
		protected void AfterAll(MethodInfo method) { fixtureSink.AfterAll(method); }
		protected void Unintresting(MethodInfo method) { fixtureSink.Unintresting(method); }
		protected void Test(MethodInfo method, ExpectedTestResult expectedResult) { testSink.Test(method, expectedResult); }
		protected void RowTest(MethodInfo method, IEnumerable<IRowData> rows) { testSink.RowTest(method, rows); }
		protected void RowSource(MethodInfo method) { testSink.RowSource(method); }
	}
}