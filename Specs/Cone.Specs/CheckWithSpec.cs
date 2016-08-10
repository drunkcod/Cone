namespace Cone
{
	[Describe(typeof(Check), "With")]
	public class CheckWithSpec
	{
		public void ensures_non_null_value() {
			var theObject = (string)null;
			Check.Exception<CheckFailed>(() => Check.With(() => theObject));
		}

		public void can_check_result_object() {
			var theObject = "HelloWorld!";
			Check.With(() => theObject)
				.That(result => result.Length == 11); 
		}
		public void can_chain_checks() {
			var theObject = "HelloWorld!";
			Check.With(() => theObject)
				.That(
					its => its.Length == 11,
					x => x.ToUpper() == "HELLOWORLD!"
				); 
		}
	}
}