using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContextAttribute : Attribute
    {
        public readonly string Context;
        public string Category;

        public ContextAttribute(string context) {
            Context = context;
        }
    }
}
