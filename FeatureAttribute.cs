using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class FeatureAttribute : ContextAttribute, IFixtureDescription
    {
        public FeatureAttribute(string featureName) : base(featureName) { }
        public string SuiteName { get { return string.Empty; } }

        public string TestName { get { return Context; } }
    }
}
