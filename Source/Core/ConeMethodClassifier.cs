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

	public class ConeMethodClassifier : MethodClassifier
	{
        public ConeMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink)
		{ }

        protected override void ClassifyCore(MethodInfo method) {
            if(method.AsConeAttributeProvider().Has<IRowData>(rows => RowTest(method, rows)))
                return;

            var parameters = method.GetParameters();
            switch(parameters.Length) {
                case 0: Niladic(method); break;
                case 1: Monadic(method, parameters[0]); break;
                default: Unintresting(method); break;
            }
        }

        void Niladic(MethodInfo method) {
            if(typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType)) {
                RowSource(method);
                return;
            }

            bool sunk = false;
            var attributes = method.GetCustomAttributes(true);
            for(int i = 0; i != attributes.Length; ++i) {
                var item = attributes[i];
                if(item is BeforeAllAttribute) {
                    BeforeAll(method);
                    sunk = true;
                }
                if(item is BeforeEachAttribute) {
                    BeforeEach(method);
                    sunk = true;
                }
                if(item is AfterEachAttribute) {
                    AfterEach(method);
                    sunk = true;
                }
                if(item is AfterAllAttribute) {
                    AfterAll(method);
                    sunk = true;
                }
            }
            if(sunk)
                return;
            
            Test(method, ExpectedTestResult.None);
        }

        void Monadic(MethodInfo method, ParameterInfo parameter) {
            if(typeof(ITestResult).IsAssignableFrom(parameter.ParameterType) 
                && method.AsConeAttributeProvider().Has<AfterEachAttribute>()) {
                AfterEachWithResult(method);
            }
            else Unintresting(method);
        }
    }
}
