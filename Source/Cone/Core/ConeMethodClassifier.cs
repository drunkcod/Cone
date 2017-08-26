using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
{
	public class ConeMethodClassifier : MethodClassifier
	{
		public ConeMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink)
		{ }

		protected override void ClassifyCore(MethodInfo method) {
			if (method.AsConeAttributeProvider().Has<IRowData>(rows => RowTest(method, rows)))
				return;

			var parameters = method.GetParameters();
			switch (parameters.Length) {
				case 0: Niladic(method); break;
				case 1: Monadic(method, parameters[0]); break;
				default: Unintresting(method); break;
			}
		}

		void Niladic(MethodInfo method) {
			if (typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType)) {
				RowSource(method);
				return;
			}

			var sunk = false;
			var attributes = method.GetCustomAttributes(true);
			for (var i = 0; i != attributes.Length; ++i) {
				var item = attributes[i];
				if (item is BeforeAllAttribute) {
					BeforeAll(method);
					sunk = true;
				}
				if (item is BeforeEachAttribute) {
					BeforeEach(method);
					sunk = true;
				}
				if (item is AfterEachAttribute) {
					AfterEach(method);
					sunk = true;
				}
				if (item is AfterAllAttribute) {
					AfterAll(method);
					sunk = true;
				}
			}
			if (!sunk && (method.ReturnType == typeof(void) || Invokable.IsWaitable(method.ReturnType)))
				Test(method, attributes, ExpectedTestResult.None);
			Unintresting(method);
		}

		void Monadic(MethodInfo method, ParameterInfo parameter) {
			if (typeof(ITestResult).IsAssignableFrom(parameter.ParameterType)
				&& method.AsConeAttributeProvider().Has<AfterEachAttribute>()) {
				AfterEachWithResult(method);
			}
			else Unintresting(method);
        }
    }
}
