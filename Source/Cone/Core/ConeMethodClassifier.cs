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

		protected override void ClassifyCore(Invokable method) {
			if (method.AsConeAttributeProvider().Has<IRowData>(rows => RowTest(method, rows)))
				return;

			var parameters = method.GetParameters();
			switch (parameters.Length) {
				case 0: Niladic(method); break;
				case 1: Monadic(method, parameters[0]); break;
				default: Unintresting(method); break;
			}
		}

		void Niladic(Invokable method) {
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
				else if (item is BeforeEachAttribute) {
					BeforeEach(method);
					sunk = true;
				}
				else if (item is AfterEachAttribute) {
					AfterEach(method);
					sunk = true;
				}
				else if (item is AfterAllAttribute) {
					AfterAll(method);
					sunk = true;
				}
			}
			if(sunk)
				return;
			if(method.ReturnType == typeof(void) || method.IsWaitable)
				Test(method, ConeTestMethodContext.Attributes(attributes));

			Unintresting(method);
		}

		void Monadic(Invokable method, ParameterInfo parameter) {
			if (typeof(ITestResult).IsAssignableFrom(parameter.ParameterType)
				&& method.AsConeAttributeProvider().Has<AfterEachAttribute>()) {
				AfterEachWithResult(method);
			}
			else Unintresting(method);
        }
    }
}
