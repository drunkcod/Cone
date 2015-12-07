using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class FeatureAttribute : ContextAttribute, IFixtureDescription
    {
        public FeatureAttribute(string featureName) : base(featureName) { }

        public string GroupAs { get; set; }

        string IFixtureDescription.SuiteName { get { return GroupAs ?? "Features"; } }

        string IFixtureDescription.SuiteType { get { return "Feature"; } }

        string IFixtureDescription.TestName { get { return Context; } }
    }
}
