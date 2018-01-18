using System.Collections.Generic;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
{
	public interface IMethodClassifier
	{
		void Classify(Invokable method);
	}

	public abstract class MethodClassifier : IMethodClassifier
	{
		readonly IConeFixtureMethodSink fixtureSink;
		readonly IConeTestMethodSink testSink;

		public MethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) {
			this.fixtureSink = fixtureSink;
			this.testSink = testSink;
		}

		public void Classify(Invokable method) {
			if(method.DeclaringType == typeof(object)) {
				Unintresting(method);
				return;
			}
			ClassifyCore(method);
		}

		protected abstract void ClassifyCore(Invokable method);

		protected void BeforeAll(Invokable method) { fixtureSink.BeforeAll(method); }
		protected void BeforeEach(Invokable method) { fixtureSink.BeforeEach(method); }
		protected void AfterEach(Invokable method) { fixtureSink.AfterEach(method); }
		protected void AfterEachWithResult(Invokable method) { fixtureSink.AfterEachWithResult(method); }
		protected void AfterAll(Invokable method) { fixtureSink.AfterAll(method); }
		protected void Unintresting(Invokable method) { fixtureSink.Unintresting(method); }
		protected void Test(Invokable method, IEnumerable<object> attributes, ConeTestMethodContext context) { testSink.Test(method, attributes, context); }
		protected void RowTest(Invokable method, IEnumerable<IRowData> rows) { testSink.RowTest(method, rows); }
		protected void RowSource(Invokable method) { testSink.RowSource(method); }
	}
}