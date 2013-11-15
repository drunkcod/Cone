using System;

namespace Cone.Samples.Failures
{
    [Feature("Failure")]
    public class FailureMessagesFeature
    {
        public void string_example() { Check.That(() => "Hello World".Length == 3); }

        public int TheAnswer = 42;

        public void member_access_example() { Check.That(() => TheAnswer == 7); }

		public void error_before_verification() {
			DoDie();
		}

		public void error_during_verification() {
			Check.That(() => NestedDeath());
		}

		bool NestedDeath() {
			return DoDie();
		}

		bool DoDie() {
			throw new Exception();
		}

		public void I_fail_because_I_devide_by_zero_in_verify() 
		{ 
			int zero = 0; 
			Check.That(() => 2 / zero == 3); 
		}
    }

	[Feature("BeforeAll failure")]
	public class BeforeAllFailure 
	{
		[BeforeAll]
		public void DieDieDie() {
			throw new Exception();
		}
		
		public void setup_fails() { }
	}

	[Feature("AfterEach failure")]
	public class AfterEachFailure 
	{
		[AfterEach]
		public void DieDieDie() {
			throw new Exception();
		}
		
		public void cleanup_fails() { }
	}

	[Feature("AfterAll failure")]
	public class AfterAllFailure 
	{
		[AfterAll]
		public void DieDieDie() {
			throw new Exception();
		}
		
		public void cleanup_fails() { }
	}

	[Feature("Test & AfterEach failure")]
	public class TestAndAfterFailure
	{
		public void fail() { throw new Exception(); }

		[AfterEach]
		public void DieDieDie() {
			throw new Exception();
		}
	}

}
