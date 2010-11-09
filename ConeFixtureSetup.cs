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
        [Flags]
        enum MethodMarks
        {
            None,
            Test = 1,
            BeforeAll = 1 << 2,
            BeforeEach = 1 << 3,
            AfterEach = 1 << 4,
            AfterAll = 1 << 5,
            AfterEachWithResult = 1 << 6,
            RowSource = 1 << 7
        }

        MethodInfo[] methods;
        MethodMarks[] marks;
        int beforeAllCount, beforeEachCount, afterEachCount, afterEachWithResultCount, afterAllCount, rowSourceCount;
        readonly IConeSuite suite;
        readonly ConeTestNamer testNamer;

        public ConeFixtureSetup(IConeSuite suite, ConeTestNamer testNamer) {
            this.suite = suite;
            this.testNamer = testNamer;
        }

        public void CollectFixtureMethods(Type type) {
            methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            marks = new MethodMarks[methods.Length];
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

                if (MarkedAs(MethodMarks.BeforeAll, i))
                    x.BeforeAll[beforeAllCount++] = method;
                if (MarkedAs(MethodMarks.BeforeEach, i))
                    x.BeforeEach[beforeEachCount++] = method;
                if (MarkedAs(MethodMarks.AfterEach, i))
                    x.AfterEach[afterEachCount++] = method;
                if (MarkedAs(MethodMarks.AfterEachWithResult, i))
                    x.AfterEachWithResult[afterEachWithResultCount++] = method;
                if (MarkedAs(MethodMarks.AfterAll, i))
                    x.AfterAll[afterAllCount++] = method;
                if (MarkedAs(MethodMarks.RowSource, i))
                    x.RowSource[rowSourceCount++] = method;
            }

            return x;
        }

        void ClassifyMethod(int index) {
            var method = methods[index];
            if (method.DeclaringType == typeof(object))
                return;
            var parms = method.GetParameters();
            if (parms.Length == 0)
                marks[index] = ClassifyNiladic(method);
            else
                marks[index] = ClassifyWithArguments(method, parms);
        }

        MethodMarks ClassifyNiladic(MethodInfo method) {
            var attributes = method.GetCustomAttributes(true);
            var marks = MethodMarks.None;
            for (int i = 0; i != attributes.Length; ++i) {
                var x = attributes[i];
                if (x is BeforeEachAttribute) {
                    marks |= MethodMarks.BeforeEach;
                    ++beforeEachCount;
                } else if (x is AfterEachAttribute) {
                    marks |= MethodMarks.AfterEach;
                    ++afterEachCount;
                } else if (x is BeforeAllAttribute) {
                    marks |= MethodMarks.BeforeAll;
                    ++beforeAllCount;
                } else if (x is AfterAllAttribute) {
                    marks |= MethodMarks.AfterAll;
                    ++afterAllCount;
                }
            }
            if (marks != MethodMarks.None)
                return marks;
            if(method.ReturnType != typeof(void))
                return ClassifyRowSource(method);

            AddTestMethod(method);
            return MethodMarks.Test;
        }

        void ResetCounts() {
            beforeAllCount = beforeEachCount = afterEachCount = afterEachWithResultCount = afterAllCount = rowSourceCount = 0;
        }

        MethodMarks ClassifyWithArguments(MethodInfo method, ParameterInfo[] parms) {
            if (!method.Has<RowAttribute>(rows => AddRowTest(method, rows))
            && parms.Length == 1
            && typeof(ITestResult).IsAssignableFrom(parms[0].ParameterType)
            && method.Has<AfterEachAttribute>()) {
                ++afterEachWithResultCount;
                return MethodMarks.AfterEachWithResult;
            } else return MethodMarks.None;
        }

        MethodMarks ClassifyRowSource(MethodInfo method){
            if(!typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType))
                return MethodMarks.None;
            ++rowSourceCount;
            return MethodMarks.RowSource;
        }

        bool MarkedAs(MethodMarks mark, int index) {
            return (marks[index] & mark) != 0;
        }

        void AddTestMethod(MethodInfo method) { suite.AddTestMethod(testNamer.NameFor(method), method); }
        void AddRowTest(MethodInfo method, RowAttribute[] rows) { suite.AddRowTest(testNamer.NameFor(method), method, rows); }
    }
}
