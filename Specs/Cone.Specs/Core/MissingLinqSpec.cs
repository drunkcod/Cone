using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Core
{
	[Describe(typeof(MissingLinq))]
	public class MissingLinqSpec
	{
		public void IsEmpty_sequence() {
			Check.That(
				() => Enumerable.Empty<object>().IsEmpty(),
				() => Enumerable.Range(0, 10).IsEmpty() == false);
		}
	}
}
