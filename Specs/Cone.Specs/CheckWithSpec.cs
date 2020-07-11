using System;
using CheckThat;

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

		public void with_value_type() =>
			Check.With(() => 42).That(theAnswer => theAnswer == 42);

		public void with_given_in_expression() =>
			Check.With(() => 1).That(x => x + 0 == 1);

		public void array_length_check() =>
			Check.With(() => new [] { 1, 2, 3 }).That(arr => arr.Length == 3);

		public void array_element() => 
			Check.With(() => new [] { 1, 2, 3 }).That(arr => arr[1] == 2);

		class MyThing
		{
			public byte[] MyBytes;
		}

		public void member_array() =>
			Check.With(() => new MyThing { MyBytes = new byte[] { 1, 2, 3 } })
			.That(
				x => x.MyBytes.Length == 3,
				x => x.MyBytes[1] == 2);

		public void method_call() =>
			Check.With(() => "Hello").That(x => x.Equals("Hello"));

		public void booleans() => 
			Check.With(() => new { IsOk = true }).That(x => x.IsOk);
	}
}