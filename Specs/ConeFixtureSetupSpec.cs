using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Moq;

namespace Cone
{
    [Describe(typeof(ConeFixtureSetup))]
    public class ConeFixtureSetupSpec
    {
        class DerivedFixture : SampleFixture 
        {
            [BeforeAll]
            public void DerivedBeforeAll() {}

            [BeforeEach]
            public void DerivedBeforeEach() {}

            [AfterEach]
            public void DerivedAfterEach() {}          

            [AfterEach]
            public void DerivedAfterEachWithResult(ITestResult result) {}

            [AfterAll]
            public void DerivedAfterAll() {}
        }

        [Context("base methods before derived ones")]
        public class DerivedFirst
        {
            ConeFixtureMethods FixtureMethods;
    
            [BeforeAll]
            public void GetFixtureMethods() {
                var setup = new ConeFixtureSetup(new Mock<IConeTestMethodSink>().Object);
                setup.CollectFixtureMethods(typeof(DerivedFixture));
                FixtureMethods = setup.GetFixtureMethods();
            }

            public void base_BeforeAll_before_derived() {
                Verify.That(() => 
                    FixtureMethods.BeforeAll.IndexOf(Base(x => x.BeforeAll())) <
                    FixtureMethods.BeforeAll.IndexOf(Derived(x => x.DerivedBeforeAll())));
            }

            public void base_BeforeEach_before_derived() {
                Verify.That(() => 
                    FixtureMethods.BeforeEach.IndexOf(Base(x => x.BeforeEach())) <
                    FixtureMethods.BeforeEach.IndexOf(Derived(x => x.DerivedBeforeEach())));
            }
        }

        static MethodInfo Base(Expression<Action<SampleFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
        static MethodInfo Derived(Expression<Action<DerivedFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
    }
}
