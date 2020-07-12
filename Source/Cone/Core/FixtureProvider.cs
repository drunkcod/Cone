using System;
using System.Linq;
using System.Reflection;
using CheckThat.Internals;

namespace Cone.Core
{
	public abstract class FixtureProvider
	{
		public abstract object NewFixture(Type fixtureType);
		
		public void Release(object fixture) {
			DoCleanup(fixture);

			var asDisposable = fixture as IDisposable;
			if (asDisposable != null)
				asDisposable.Dispose();
		}

		private void DoCleanup(object fixture) {
			var fixtureType = fixture.GetType();
			var fields = fixtureType.GetFields(BindingFlags.Public | BindingFlags.Instance);
			
			fields.Where(x => typeof(ITestCleanup).IsAssignableFrom(x.FieldType)).ForEach(item => {
				var cleaner = (ITestCleanup)item.GetValue(fixture);
				cleaner.Cleanup();
			});
		}
	}
}