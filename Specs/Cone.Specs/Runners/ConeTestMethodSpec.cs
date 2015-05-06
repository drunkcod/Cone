using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			public async Task AsyncTask() { }

			public void SyncVoid() { }
		}

		public void detects_async_method()
		{
			var method = new ConeTestMethod(null, typeof(MyType).GetMethod("AsyncVoid"));
			Check.That(() => method.IsAsync);
		}

		public void detects_sync_method()
		{
			var method = new ConeTestMethod(null, typeof(MyType).GetMethod("SyncVoid"));
			Check.That(() => method.IsAsync == false);
		}

		[Pending]
		public async void dissallow_async_void_methods() { await Task.Factory.StartNew(() => {});}
	}
}
