using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Moq;

namespace Cone
{
    class ConeFixtureMethods : IConeFixtureMethodSink
    {
        public List<MethodInfo> BeforeAll = new List<MethodInfo>();
        public List<MethodInfo> BeforeEach = new List<MethodInfo>();
        public List<MethodInfo> AfterEach = new List<MethodInfo>();
        public List<MethodInfo> AfterAll = new List<MethodInfo>();
        public List<MethodInfo> AfterEachWithResult = new List<MethodInfo>();

        void IConeFixtureMethodSink.Unintresting(MethodInfo method) { }
        void IConeFixtureMethodSink.BeforeAll(MethodInfo method) { BeforeAll.Add(method); }
        void IConeFixtureMethodSink.BeforeEach(MethodInfo method) { BeforeEach.Add(method); }
        void IConeFixtureMethodSink.AfterEach(MethodInfo method) { AfterEach.Add(method); }
        void IConeFixtureMethodSink.AfterEachWithResult(MethodInfo method) { AfterEachWithResult.Add(method); }
        void IConeFixtureMethodSink.AfterAll(MethodInfo method) { AfterAll.Add(method); }
    }

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
                FixtureMethods = new ConeFixtureMethods();
				var testSink = new Mock<IConeTestMethodSink>().Object;
                var setup = new ConeFixtureSetup(FixtureMethods, testSink, new ConeMethodClassifier(FixtureMethods, testSink));
                setup.CollectFixtureMethods(typeof(DerivedFixture));
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

        [Context("virtual fixture methods")]
        public class VirtualFixtureMethods
        {
            class BaseFixture
            {
                [BeforeEach]
                public virtual void BeforeEach() { }
            }

            class DerivedFixture : BaseFixture
            {
                public override void BeforeEach() { }
            }

            public void are_classified_only_once() {
                var methods = new ConeFixtureMethods();
				var testSink = new Mock<IConeTestMethodSink>().Object;
                var setup = new ConeFixtureSetup(methods, testSink, new ConeMethodClassifier(methods, testSink));
                setup.CollectFixtureMethods(typeof(DerivedFixture));
                Verify.That(() => methods.BeforeEach.Count == 1);
            }
        }

        static MethodInfo Base(Expression<Action<SampleFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
        static MethodInfo Derived(Expression<Action<DerivedFixture>> x) { return ((MethodCallExpression)x.Body).Method; }
    }
}
