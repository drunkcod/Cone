using System.Reflection;
using NUnit.Core;

namespace Cone.Addin
{
    class ReportingConeTestMethod : ConeTestMethod
    {
        readonly MethodInfo[] afters;

        public ReportingConeTestMethod(MethodInfo method, object[] parameters, ConeSuite suite, string name, MethodInfo[] afters)
            : base(method, parameters, suite, suite, name) {
            this.afters = afters;
        }

        protected override void AfterCore(TestResult testResult) {
            var parms = new[] { new NUnitTestResultAdapter(testResult) };
            for (int i = 0; i != afters.Length; ++i)
                try {
                    afters[i].Invoke(Fixture, parms);
                } catch (TargetInvocationException e) {
                    throw e.InnerException;
                }
        }
    }
}
