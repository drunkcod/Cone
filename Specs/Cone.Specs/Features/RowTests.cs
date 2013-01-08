using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Features
{
	[Feature("Row tests")]
	public class RowTests
	{
		public enum MyEnum 
		{
			Default, First
		}

		[DisplayAs("{0}", Heading = "enum values")
		,Row(MyEnum.Default)
		,Row(-1)]
		public void enums(MyEnum value) { }
	}
}
