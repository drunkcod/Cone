using System;

namespace Cone.Core
{
	public class LambdaObjectProvider : FixtureProvider
	{
		private readonly Func<Type, object> newFixture;

		public LambdaObjectProvider(Func<Type, object> newFixture) {
			this.newFixture = newFixture;
		}

		public override object NewFixture(Type fixtureType) {
			return newFixture(fixtureType);
		}
	}
}