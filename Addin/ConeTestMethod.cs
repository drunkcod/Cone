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
       
        internal void Invoke(object fixture, object[] parameters) { method.Invoke(fixture, parameters); }
        
        internal string NameFor(ConeTestNamer namer, object[] parameters) { return namer.NameFor(method, parameters); }

        public override void Run(ITestResult testResult) { Invoke(Fixture, null); }
    }
}
