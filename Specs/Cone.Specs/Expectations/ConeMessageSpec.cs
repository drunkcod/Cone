namespace Cone.Expectations
{
	[Describe(typeof(ConeMessage))]
	public class ConeMessageSpec
	{
		public void roundtrip_newline() =>
			Check.That(() => ConeMessage.Parse("A\nB\n").ToString() == "A\nB\n");

		public void combine_newlines() =>
			Check.That(() => ConeMessage.Combine(
				ConeMessage.Parse("A\n"),
				new[]{ ConeMessageElement.NewLine }).ToString() == "A\n\n");
	}
}
