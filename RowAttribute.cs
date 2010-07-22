using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RowAttribute : Attribute
    {
        public readonly object[] Parameters;
        public bool IsPending;
        public RowAttribute(params object[] args) { Parameters = args; }
    }
}
