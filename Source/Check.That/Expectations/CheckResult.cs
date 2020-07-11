using CheckThat.Internals;

namespace CheckThat.Expectations
{
	public struct CheckResult
	{
		public readonly bool IsSuccess;
		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;

		public CheckResult(bool success, Maybe<object> actual, Maybe<object> expected) {
			this.IsSuccess = success;
			this.Actual = actual;
			this.Expected = expected;
		}

		public static bool operator==(CheckResult left, CheckResult right) =>
			left.IsSuccess == right.IsSuccess 
			&& left.Actual == right.Actual
			&& left.Expected == right.Expected;

		public static bool operator!=(CheckResult left, CheckResult right) => 
			!(left == right);

		public override string ToString() =>
			string.Format("{{Success: {0}, Actual: {1}}}", IsSuccess, Actual);

		public override bool Equals(object obj) =>
			obj is CheckResult && (CheckResult)obj == this;

		public override int GetHashCode() => 
			Actual.GetHashCode();
	}
}