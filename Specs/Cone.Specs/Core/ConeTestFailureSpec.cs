using System;

namespace Cone.Core
{
	[Describe(typeof(ConeTestFailure))]
	public class ConeTestFailureSpec
	{
		public void Unwraps_AggregateException_with_single_error() {
			var inner = new Exception();

			Check.That(() => ConeTestFailure.Unwrap(new AggregateException(inner)) == inner);		
		}
	}
}