using System;

namespace Cone.Samples.Failures
{
    [Feature("Failure")]
    public class FailureMessagesFeature
    {
        public void string_example() { Verify.That(() => "Hello World".Length == 3); }

        public int TheAnswer = 42;

        public void member_access_example() { Verify.That(() => TheAnswer == 7); }

		public void error_before_verification() {
			DoDie();
		}

		public void error_during_verification() {
			Verify.That(() => NestedDeath());
		}

		bool NestedDeath() {
			return DoDie();
		}

		bool DoDie() {
			throw new Exception();
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
