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

        public ConeTestMethod(MethodInfo method, Test suite, TestExecutor testExecutor, string name)
            : base(suite, testExecutor, name) {
            this.method = method;
        }

        public MethodInfo Method { get { return method; } }

        public override void Run(ITestResult testResult) { Method.Invoke(Fixture, null); }
    }
}
