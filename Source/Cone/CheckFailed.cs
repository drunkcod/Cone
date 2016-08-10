using System;
using System.Linq;
using Cone.Core;

namespace Cone
{
	public class FailedExpectation
	{
		public FailedExpectation(string message) : this(message, Maybe<object>.None, Maybe<object>.None) { }
		public FailedExpectation(string message, Maybe<object> actual, Maybe<object> expected) {
			this.Message = message;
			this.Actual = actual;
			this.Expected = expected;
		}

		public readonly string Message;
		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;
	}

	[Serializable]
	public class CheckFailed : Exception
	{
		public CheckFailed(string message) : this(string.Empty, new[]{ new FailedExpectation(message) }, null) { }

		public CheckFailed(string context, FailedExpectation fail, Exception innerException) : this(context, new[] { fail }, innerException) { }

		public CheckFailed(string context, FailedExpectation[] fails, Exception innerException) : base(fails.Select(x => x.Message).Join("\n"), innerException) {
			this.Context = context;
			this.Failures = fails;
		}

		public readonly string Context;
		public readonly FailedExpectation[] Failures;

    }
}
