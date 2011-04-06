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
        MethodInfo[] methods;
        ConeMethodClass[] marks;
        int beforeAllCount, beforeEachCount, afterEachCount, afterEachWithResultCount, afterAllCount, rowSourceCount;
        readonly IConeSuite suite;
        readonly ConeTestNamer testNamer;

        public ConeFixtureSetup(IConeSuite suite, ConeTestNamer testNamer) {
            this.suite = suite;
            this.testNamer = testNamer;
            this.classifier = new ConeMethodClassifier();
            classifier.Test += (_, e) => AddTestMethod(e.Method);
        }

        public void CollectFixtureMethods(Type type) {
            methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            marks = new ConeMethodClass[methods.Length];
            ResetCounts();

            for (int i = 0; i != methods.Length; ++i)
                ClassifyMethod(i);
        }

        public ConeFixtureMethods GetFixtureMethods() {
            var x = new ConeFixtureMethods();
            x.BeforeAll = new MethodInfo[beforeAllCount];
            x.BeforeEach = new MethodInfo[beforeEachCount];
            x.AfterEach = new MethodInfo[afterEachCount];
            x.AfterEachWithResult = new MethodInfo[afterEachWithResultCount];
            x.AfterAll = new MethodInfo[afterAllCount];
            x.RowSource = new MethodInfo[rowSourceCount];
            
            ResetCounts();

            for (int i = 0; i != methods.Length; ++i) {
                var method = methods[i];

                if (MarkedAs(ConeMethodClass.BeforeAll, i))
                    x.BeforeAll[beforeAllCount++] = method;
                if (MarkedAs(ConeMethodClass.BeforeEach, i))
                    x.BeforeEach[beforeEachCount++] = method;
                if (MarkedAs(ConeMethodClass.AfterEach, i))
                    x.AfterEach[afterEachCount++] = method;
                if (MarkedAs(ConeMethodClass.AfterEachWithResult, i))
                    x.AfterEachWithResult[afterEachWithResultCount++] = method;
                if (MarkedAs(ConeMethodClass.AfterAll, i))
                    x.AfterAll[afterAllCount++] = method;
                if (MarkedAs(ConeMethodClass.RowSource, i))
                    x.RowSource[rowSourceCount++] = method;
            }

            return x;
        }

        void ClassifyMethod(int index) {            
            var method = methods[index];
            var mark = Classify(method);            
            for(var x = (int)mark; x != 0; x = x & (x - 1)) {
                switch(x & ~(x - 1)) {
                    case (int)ConeMethodClass.RowTest:
                        method.Has<RowAttribute>(rows => AddRowTest(method, rows));
                        break;
                    case (int)ConeMethodClass.RowSource:
                        ++rowSourceCount;
                        break;
                    case (int)ConeMethodClass.BeforeAll:
                        ++beforeAllCount;
                        break;
                    case (int)ConeMethodClass.BeforeEach:
                        ++beforeEachCount;
                        break;
                    case (int)ConeMethodClass.AfterEach: 
                        ++afterEachCount;
                        break;
                    case (int)ConeMethodClass.AfterEachWithResult:
                        ++afterEachWithResultCount;
                        break;
                    case (int)ConeMethodClass.AfterAll:
                        ++afterAllCount;
                        break;
                }
            }            

            marks[index] = mark;
        }

        void ResetCounts() {
            beforeAllCount = beforeEachCount = afterEachCount = afterEachWithResultCount = afterAllCount = rowSourceCount = 0;
        }

        bool MarkedAs(ConeMethodClass mark, int index) {
            return (marks[index] & mark) != 0;
        }

        void AddTestMethod(MethodInfo method) {
            suite.AddTestMethod(new ConeMethodThunk(method, testNamer)); 
        }
        
        void AddRowTest(MethodInfo method, RowAttribute[] rows) { suite.AddRowTest(testNamer.NameFor(method), method, rows); }
        
        ConeMethodClass Classify(MethodInfo method) { return classifier.Classify(method); }
    }
}
