using System;

namespace Cone
{
    public class ExpectationFailedException : Exception
    {
        public ExpectationFailedException(string message) : base(message) { }
    }
}
