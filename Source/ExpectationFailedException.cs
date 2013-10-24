using System;
using System.Linq;
using Cone.Core;
using System.Collections.Generic;

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
    public class ExpectationFailedException : Exception
    {
        public ExpectationFailedException(string message) : this(new[]{ new FailedExpectation(message) }, null) { }
        public ExpectationFailedException(IEnumerable<FailedExpectation> fails, Exception innerException) : base(string.Join("\n", fails.Select(x => x.Message).ToArray()), innerException) { 
			this.Failures = fails.ToList();
		}

		public readonly List<FailedExpectation> Failures;

    }
}
