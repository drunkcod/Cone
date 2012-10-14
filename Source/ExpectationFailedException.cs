using System;

namespace Cone
{
    [Serializable]
    public class ExpectationFailedException : Exception
    {
        public ExpectationFailedException(string message) : base(message) { }
    }
}
