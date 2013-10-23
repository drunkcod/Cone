using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Core
{
	[Describe(typeof(ConeTestName))]
	public class ConeTestNameSpec
	{
		public void trims_leading_whitespace() {
			Verify.That(() => new ConeTestName("<context>", " <name>").Name == "<name>");
		}
	}
}
