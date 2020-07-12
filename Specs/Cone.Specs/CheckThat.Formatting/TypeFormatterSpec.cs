using System;
using Cone;

namespace CheckThat.Formatting
{
	[Describe(typeof(TypeFormatterSpec))]
	public class TypeFormatterSpec
	{
		[DisplayAs("{0}", Heading = "Format(type)")
		,Row(typeof(object), "object")
		,Row(typeof(long), "long")
		,Row(typeof(float), "float")
		,Row(typeof(double), "double")
		,Row(typeof(int?), "int?")
		,Row(typeof(Nullable<float>), "float?")]
		public void check(Type type, string expected) {
			Check.That(() => TypeFormatter.Format(type) == expected);
		}
	}
}
