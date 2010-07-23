using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;
using System.Collections;

namespace Cone.Addin
{
    class ConeTestMethod : ConeTest
    {
        readonly MethodInfo method;

        public ConeTestMethod(MethodInfo method, Test suite, string name)
            : base(suite, name) {
            this.method = method;
        }

        public MethodInfo Method { get { return method; } }

        protected override void Run(TestResult testResult) {
            Method.Invoke(Fixture, null);
            testResult.Success();
        }
    }
}
