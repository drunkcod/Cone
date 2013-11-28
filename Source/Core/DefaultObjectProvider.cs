using System;

namespace Cone.Core
{
	public class DefaultObjectProvider : ObjectProvider
	{
		public override object NewFixture(Type fixtureType) { 
			if(IsStatic(fixtureType))
				return null;
			var ctor = fixtureType.GetConstructor(Type.EmptyTypes);
			if(ctor == null)
				throw new NotSupportedException("No compatible constructor found for " + fixtureType.FullName);
			return ctor.Invoke(null);
		}

		private static bool IsStatic(Type fixtureType) {
			return fixtureType.IsSealed && fixtureType.GetConstructors().Length == 0;
		}
	}
}