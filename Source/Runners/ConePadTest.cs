using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
    class ConePadTest : IConeTest
    {
        readonly ITestName name;
        readonly IConeFixture fixture;
        readonly MethodInfo method;
        readonly object[] args;
        readonly IConeAttributeProvider attributes;

        public ConePadTest(ITestName name, IConeFixture fixture, MethodInfo method, object[] args, IConeAttributeProvider attributes) {
            this.name = name;
            this.fixture = fixture;
            this.method = method;
            this.args = args;
            this.attributes = attributes;
        }

        public ITestName Name { get { return name; } }

        IConeAttributeProvider IConeTest.Attributes { get { return attributes; } }
        void IConeTest.Run(ITestResult result) { method.Invoke(fixture.Fixture, args); }
    }
}
