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

    public interface IMethodProvider 
    {
        IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags bindingFlags);
    }

    public class ConeFixtureSetup : IMethodProvider
    {
        readonly ConeMethodClassifier classifier;
        readonly IMethodProvider methodProvider;

        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();
        readonly List<MethodInfo> rowSources = new List<MethodInfo>(); 

        public ConeFixtureSetup(): this(null) { }

        public ConeFixtureSetup(IMethodProvider methodProvider) {
            this.methodProvider = methodProvider ?? this;
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
            foreach(var item in methodProvider.GetMethods(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                Classify(item);
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

            x.RowSource = rowSources.ToArray();

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

        ConeMethodClass Classify(MethodInfo method) { return classifier.Classify(method); }

        public IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags bindingFlags) {
            return type.GetMethods(bindingFlags);
        }
    }
}
