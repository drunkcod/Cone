using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Core
{
	[Describe(typeof(ConeFixtureMethodCollection))]
	public class ConeFixtureMethodCollectionSpec
	{
		class FakeFixture
		{
			public void Fail(){ throw new NotImplementedException(); }
			public void Fail(ITestResult _){ throw new NotImplementedException(); }
			public void Success() { ++SuccessCount; }
			public void Success(ITestResult _) { ++SuccessCount; }
 
			public int SuccessCount;
		}

		public void all_methods_are_called_in_case_of_early_failure() {
			var fixture = new FakeFixture();

			var fixtureMethods = new ConeFixtureMethodCollection();
			fixtureMethods.AfterEach(GetMethod(fixture, x => x.Fail()));
			fixtureMethods.AfterEachWithResult(GetMethod(fixture, x => x.Fail(null)));
			fixtureMethods.AfterEach(GetMethod(fixture, x => x.Success()));
			fixtureMethods.AfterEachWithResult(GetMethod(fixture, x => x.Success(null)));

			var failure = Check<FixtureException>.When(() => fixtureMethods.InvokeAfterEach(fixture, EmptyResult()));
			Check.That(
				() => failure.Count == 2,
				() => failure[0] is NotImplementedException,
				() => fixture.SuccessCount == 2);
		}

		MethodInfo GetMethod<T>(T _, Expression<Action<T>> act) {
			return ((MethodCallExpression)act.Body).Method;
		}

		private static ITestResult EmptyResult() {
			return null;
		}
	}
}
