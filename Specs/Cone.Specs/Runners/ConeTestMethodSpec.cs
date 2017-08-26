using Cone.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace Cone.Runners
{
	[Describe(typeof(ConeTestMethod))]
	public class ConeTestMethodSpec
	{
		class MyType
		{
			public async void AsyncVoid() {
				await AsyncTask();
			}

			public async Task AsyncTask() { await Task.FromResult(0); }

			public void SyncVoid() { }

			public void Parameterized(int aInt, object anObject) { }
		}

		public void detects_async_method() => Check.With(() => TestMethod(typeof(MyType).GetMethod(nameof(MyType.AsyncVoid))))
			.That(method => method.IsAsync);

		public void detects_sync_method() => Check.With(() => TestMethod(typeof(MyType).GetMethod(nameof(MyType.SyncVoid))))
			.That(method => method.IsAsync == false);
		

		[Pending]
		public async void dissallow_async_void_methods() { await Task.Factory.StartNew(() => {});}

		public void location_is_native_method_name() => Check.With(() => TestMethod(typeof(MyType).GetMethod(nameof(MyType.SyncVoid))))
			.That(method => method.Location == "System.Void Cone.Runners.ConeTestMethodSpec+MyType.SyncVoid()");

		public void location_ends_with_parameters() => Check.With(() => TestMethod(typeof(MyType).GetMethod(nameof(MyType.Parameterized))))
			.That(method => method.Location.EndsWith("(Int32 aInt, System.Object anObject)"));

		ConeTestMethod TestMethod(MethodInfo method) => new ConeTestMethod(new ConeFixture(method.DeclaringType, new string[0], new DefaultFixtureProvider()), new Invokable(method));
	}
}
