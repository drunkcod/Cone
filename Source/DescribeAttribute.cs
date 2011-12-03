using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DescribeAttribute : ContextAttribute, IFixtureDescription
    {
        public readonly Type DescribedType;

        public DescribeAttribute(Type type) : this(type, string.Empty) { }

        public DescribeAttribute(Type type, string context): base(context) {
            DescribedType = type;
        }

        public string SuiteName { get { return DescribedType.Namespace; } }

        public string SuiteType { get { return "Description"; } }

        public override string Context { get {  return TestName; } }

        public string TestName {
            get {
                if (string.IsNullOrEmpty(rawContext))
                    return DescribedType.Name;
                return DescribedType.Name + " - " + rawContext;
            }
        }
    }
}