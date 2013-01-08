using System;
using Cone.Core;

namespace Cone
{
    [Serializable]
    public class ExpectationFailedException : Exception
    {
        public ExpectationFailedException(string message, Maybe<object> actual, Maybe<object> expected) : base(message) { 
			this.Actual = actual;
			this.Expected = expected;
		}

		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;
    }
}
