using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContextAttribute : Attribute
    {
        public readonly string Context;

        public ContextAttribute(string context) {
            Context = context;
        }
    }
}
