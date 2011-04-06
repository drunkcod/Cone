using System;
using System.Reflection;
using System.Collections.Generic;

namespace Cone
{
    public class ConeFixtureMethods
    {
        public MethodInfo[] BeforeAll;
        public MethodInfo[] BeforeEach;
        public MethodInfo[] AfterEach;
        public MethodInfo[] AfterAll;
        public MethodInfo[] AfterEachWithResult;
        public MethodInfo[] RowSource;
    }

    public class ConeFixtureSetup
    {
        readonly ConeMethodClassifier classifier;
        readonly IConeSuite suite;
        readonly ConeTestNamer testNamer;

        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();
        readonly List<MethodInfo> rowSources = new List<MethodInfo>(); 

        public ConeFixtureSetup(IConeSuite suite, ConeTestNamer testNamer) {
            this.suite = suite;
            this.testNamer = testNamer;
            this.classifier = new ConeMethodClassifier();
            classifier.Test += (_, e) => AddTestMethod(e.Method);
            classifier.RowTest += (_, e) => AddRowTest(e.Method, e.Rows);
            classifier.BeforeAll += (_, e) => beforeAll.Add(e.Method);
            classifier.BeforeEach += (_,e) => beforeEach.Add(e.Method);
            classifier.AfterEach += (_, e) => afterEach.Add(e.Method);
            classifier.AfterEachWithResult += (_, e) => afterEachWithResult.Add(e.Method);
            classifier.AfterAll += (_, e) => afterAll.Add(e.Method);
            classifier.RowSource += (_, e) => rowSources.Add(e.Method);            
        }

        public void CollectFixtureMethods(Type type) {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            for (int i = 0; i != methods.Length; ++i)
                Classify(methods[i]);
        }

        public ConeFixtureMethods GetFixtureMethods() {
            var x = new ConeFixtureMethods();
            x.BeforeAll = beforeAll.ToArray();
            x.BeforeEach = beforeEach.ToArray();
            x.AfterEach = afterEach.ToArray();
            x.AfterEachWithResult = afterEachWithResult.ToArray();
            x.AfterAll = afterAll.ToArray();
            x.RowSource = rowSources.ToArray();

            return x;
        }

        void AddTestMethod(MethodInfo method) {
            suite.AddTestMethod(new ConeMethodThunk(method, testNamer)); 
        }
        
        void AddRowTest(MethodInfo method, RowAttribute[] rows) { suite.AddRowTest(testNamer.NameFor(method), method, rows); }
        
        ConeMethodClass Classify(MethodInfo method) { return classifier.Classify(method); }
    }
}
