using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DescribeAttribute : ContextAttribute
    {
        public readonly Type DescribedType;

        public DescribeAttribute(Type type) : this(type, string.Empty) { }
        public DescribeAttribute(Type type, string context): base(context) {
            DescribedType = type;
        }

        public string ParentSuiteName { get { return DescribedType.Namespace; } }
        public string TestName {
            get {
                if (string.IsNullOrEmpty(Context))
                    return DescribedType.Name;
                return DescribedType.Name + " - " + Context;
            }
        }
    }
}