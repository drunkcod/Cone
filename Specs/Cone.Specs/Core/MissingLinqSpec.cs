using System.Linq;
using CheckThat;

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

		public void Join_string() {
			Check.That(
				() => new []{ "Hello" }.Join(",") == "Hello",
				() => new []{ "Hello", "World" }.Join(" ") == "Hello World");
		}
	}
}
