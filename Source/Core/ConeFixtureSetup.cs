using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixtureMethods
    {
        public MethodInfo[] BeforeAll;
        public MethodInfo[] BeforeEach;
        public MethodInfo[] AfterEach;
        public MethodInfo[] AfterAll;
        public MethodInfo[] AfterEachWithResult;
    }

    public interface IMethodProvider 
    {
        IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags bindingFlags);
    }

    public class ConeFixtureSetup : IMethodProvider, IConeFixtureMethodSink
    {
        readonly ConeMethodClassifier classifier;
        readonly IMethodProvider methodProvider;

        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();

        public ConeFixtureSetup(IConeTestMethodSink testSink): this(null, testSink) { }

        public ConeFixtureSetup(IMethodProvider methodProvider, IConeTestMethodSink testSink) {
            this.methodProvider = methodProvider ?? this;
            this.classifier = new ConeMethodClassifier(this, testSink);
        }

        public void CollectFixtureMethods(Type type) {
            methodProvider.GetMethods(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .ForEach(Classify);
        }

        public ConeFixtureMethods GetFixtureMethods() {
            var x = new ConeFixtureMethods();
            x.BeforeAll = beforeAll.ToArray();
            Array.Sort(x.BeforeAll, BaseBeforeDerived);

            x.BeforeEach = beforeEach.ToArray();
            Array.Sort(x.BeforeEach, BaseBeforeDerived);

            x.AfterEach = afterEach.ToArray();
            Array.Sort(x.AfterEach, DerivedBeforeBase);

            x.AfterEachWithResult = afterEachWithResult.ToArray();
            Array.Sort(x.AfterEachWithResult, DerivedBeforeBase);

            x.AfterAll = afterAll.ToArray();
            Array.Sort(x.AfterAll, DerivedBeforeBase);

            return x;
        }

        static int BaseBeforeDerived(MethodInfo x, MethodInfo y) {
            if(x.DeclaringType == y.DeclaringType)
                return x.Name.CompareTo(y.Name); 
            if(x.DeclaringType.IsAssignableFrom(y.DeclaringType))
                return -1;
            return 1;
        }

        static int DerivedBeforeBase(MethodInfo x, MethodInfo y) {
            return -BaseBeforeDerived(x, y);
        }

        void Classify(MethodInfo method) { classifier.Classify(method); }

        public IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags bindingFlags) {
            return type.GetMethods(bindingFlags);
        }

        void IConeFixtureMethodSink.Unintresting(MethodInfo method) { }

        void IConeFixtureMethodSink.BeforeAll(MethodInfo method) { beforeAll.Add(method); }

        void IConeFixtureMethodSink.BeforeEach(MethodInfo method) { beforeEach.Add(method); }

        void IConeFixtureMethodSink.AfterEach(MethodInfo method) { afterEach.Add(method); }

        void IConeFixtureMethodSink.AfterEachWithResult(MethodInfo method) { afterEachWithResult.Add(method); }

        void IConeFixtureMethodSink.AfterAll(MethodInfo method) { afterAll.Add(method); }
    }
}
