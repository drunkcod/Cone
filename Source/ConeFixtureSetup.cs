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

        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();
        readonly List<MethodInfo> rowSources = new List<MethodInfo>(); 

        public ConeFixtureSetup() {
            this.classifier = new ConeMethodClassifier();
            classifier.BeforeAll += (_, e) => beforeAll.Add(e.Method);
            classifier.BeforeEach += (_,e) => beforeEach.Add(e.Method);
            classifier.AfterEach += (_, e) => afterEach.Add(e.Method);
            classifier.AfterEachWithResult += (_, e) => afterEachWithResult.Add(e.Method);
            classifier.AfterAll += (_, e) => afterAll.Add(e.Method);
            classifier.RowSource += (_, e) => rowSources.Add(e.Method);            
        }

        public event EventHandler<MethodClassEventArgs> Test {
            add { classifier.Test += value; }
            remove { classifier.Test -= value; }
        }

        public event EventHandler<RowTestClassEventArgs> RowTest {
            add { classifier.RowTest += value; }
            remove { classifier.RowTest -= value; }
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

        ConeMethodClass Classify(MethodInfo method) { return classifier.Classify(method); }
    }
}
