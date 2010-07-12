using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DescribeAttribute : Attribute
    {
        public readonly Type DescribedType;
        public readonly string Context;

        public DescribeAttribute(Type type) : this(type, string.Empty) { }
        public DescribeAttribute(Type type, string context) {
            DescribedType = type;
            Context = context;
        }
    }
}