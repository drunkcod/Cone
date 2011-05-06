using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

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

        [Context("handles method sorted with derived first")]
        public class DerivedFirst : IMethodProvider
        {
            ConeFixtureMethods FixtureMethods;
    
            [BeforeAll]
            public void GetFixtureMethods() {
                var setup = new ConeFixtureSetup(this);
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

            IEnumerable<MethodInfo> IMethodProvider.GetMethods(Type type, BindingFlags bindingFlags) {
                foreach(var item in type.GetMethods(bindingFlags | BindingFlags.DeclaredOnly))
                    yield return item;
                foreach(var item in type.BaseType.GetMethods(bindingFlags))
                    yield return item;
            }
        }

        [Context("handles method sorted with derived last")]
        public class DerivedLast : IMethodProvider
        {
            ConeFixtureMethods FixtureMethods;

            [BeforeAll]
            public void GetFixtureMethods() {
                var setup = new ConeFixtureSetup(this);
                setup.CollectFixtureMethods(typeof(DerivedFixture));
                FixtureMethods = setup.GetFixtureMethods();
            }

            public void base_AfterEach_after_derived() {
                Verify.That(() => 
                    FixtureMethods.AfterEach.IndexOf(Base(x => x.AfterEach())) >
                    FixtureMethods.AfterEach.IndexOf(Derived(x => x.DerivedAfterEach())));
            }

            public void base_AfterEach_with_result_after_derived() {
                Verify.That(() => 
                    FixtureMethods.AfterEachWithResult.IndexOf(Base(x => x.AfterEachWithResult(null))) >
                    FixtureMethods.AfterEachWithResult.IndexOf(Derived(x => x.DerivedAfterEachWithResult(null))));
            }

            public void base_AfterAll_after_derived() {
                Verify.That(() => 
                    FixtureMethods.AfterAll.IndexOf(Base(x => x.AfterAll())) >
                    FixtureMethods.AfterAll.IndexOf(Derived(x => x.DerivedAfterAll())));
            }

            IEnumerable<MethodInfo> IMethodProvider.GetMethods(Type type, BindingFlags bindingFlags) {
                foreach(var item in type.BaseType.GetMethods(bindingFlags))
                    yield return item;
                foreach(var item in type.GetMethods(bindingFlags | BindingFlags.DeclaredOnly))
                    yield return item;
            }
        }

        static MethodInfo Base(Expression<Action<SampleFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
        static MethodInfo Derived(Expression<Action<DerivedFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
    }
}
