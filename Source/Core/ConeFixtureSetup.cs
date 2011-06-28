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

    public class ConeFixtureSetup : IConeFixtureMethodSink
    {
        const BindingFlags OwnPublicMethods = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        readonly ConeMethodClassifier classifier;

        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();

        public ConeFixtureSetup(IConeTestMethodSink testSink) {
            this.classifier = new ConeMethodClassifier(this, testSink);
        }

        public void CollectFixtureMethods(Type type) {
            if(type == typeof(object))
                return;
            CollectFixtureMethods(type.BaseType);
            GetMethods(type).ForEach(Classify);
        }

        public ConeFixtureMethods GetFixtureMethods() {
            return new ConeFixtureMethods {
                BeforeAll = beforeAll.ToArray(),
                BeforeEach = beforeEach.ToArray(),
                AfterEach = afterEach.ToArray(),
                AfterEachWithResult = afterEachWithResult.ToArray(),
                AfterAll = afterAll.ToArray()
            };
        }

        void Classify(MethodInfo method) { classifier.Classify(method); }

        MethodInfo[] GetMethods(Type type) {
            return type.GetMethods(OwnPublicMethods);
        }

        void IConeFixtureMethodSink.Unintresting(MethodInfo method) { }

        void IConeFixtureMethodSink.BeforeAll(MethodInfo method) { beforeAll.Add(method); }

        void IConeFixtureMethodSink.BeforeEach(MethodInfo method) { beforeEach.Add(method); }

        void IConeFixtureMethodSink.AfterEach(MethodInfo method) { afterEach.Add(method); }

        void IConeFixtureMethodSink.AfterEachWithResult(MethodInfo method) { afterEachWithResult.Add(method); }

        void IConeFixtureMethodSink.AfterAll(MethodInfo method) { afterAll.Add(method); }
    }
}
