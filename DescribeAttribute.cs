using System;

namespace Cone
{
    public class DescribeAttribute : Attribute
    {
        public DescribeAttribute(Type type) : this(type, string.Empty) { }
        public DescribeAttribute(Type type, string context) { }
    }
}
