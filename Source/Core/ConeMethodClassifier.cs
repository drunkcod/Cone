using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    [Flags]
    public enum ConeMethodClass
    {
        Unintresting,
        Test = 1,
        RowTest = Test << 1,
        BeforeAll = RowTest << 1,
        BeforeEach = BeforeAll << 1,
        AfterEach = BeforeEach << 1,
        AfterAll = AfterEach << 1,
        AfterEachWithResult = AfterAll << 1,
        RowSource = AfterEachWithResult << 1
    }

    public class MethodClassEventArgs : EventArgs 
    {
        readonly MethodInfo method;

        public MethodClassEventArgs(MethodInfo method) {
            this.method = method;
        }

        public MethodInfo Method { get { return method; } }
    }

    public class RowTestClassEventArgs : MethodClassEventArgs 
    {
        readonly RowAttribute[] rows;

        public RowTestClassEventArgs(MethodInfo method, RowAttribute[] rows) : base(method) {
            this.rows = rows;
        }

        public RowAttribute[] Rows { get { return rows; } }
    }

    static class EventHandlerExtensions 
    {
        public static void Raise<T>(this EventHandler<T> self, object sender, T e) where T : EventArgs {
            if(self != null)
                self(sender, e);
        }
    }

    public class ConeMethodClassifier 
    {
        public event EventHandler<MethodClassEventArgs> Test;
        public event EventHandler<RowTestClassEventArgs> RowTest;
        public event EventHandler<MethodClassEventArgs> RowSource;
        public event EventHandler<MethodClassEventArgs> BeforeAll;
        public event EventHandler<MethodClassEventArgs> BeforeEach;
        public event EventHandler<MethodClassEventArgs> AfterEach;
        public event EventHandler<MethodClassEventArgs> AfterEachWithResult;
        public event EventHandler<MethodClassEventArgs> AfterAll;

        public ConeMethodClass Classify(MethodInfo method) {
            if(method.DeclaringType == typeof(object))
                return ConeMethodClass.Unintresting;
            var attributes = method.GetCustomAttributes(true);

            if(method.Has<RowAttribute>(rows => RowTest.Raise(this, new RowTestClassEventArgs(method, rows))))
                return ConeMethodClass.RowTest;
           
            var parameters = method.GetParameters();
            if(parameters.Length == 0)
                return ClassifyNiladic(method, attributes);
            else if(parameters.Length == 1)
                return ClassifyMonadic(method, attributes, parameters[0]);
            return ConeMethodClass.Unintresting;
        }

        ConeMethodClass ClassifyNiladic(MethodInfo method, object[] attributes) {
            if(typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType)) {
                RowSource.Raise(this, new MethodClassEventArgs(method));
                return ConeMethodClass.RowSource;
            }

            var mark = ConeMethodClass.Unintresting;
            for(int i = 0; i != attributes.Length; ++i) {
                var item = attributes[i];
                if(item is BeforeAllAttribute) {
                    BeforeAll.Raise(this, new MethodClassEventArgs(method));
                    mark |= ConeMethodClass.BeforeAll;
                }
                if(item is BeforeEachAttribute) {
                    BeforeEach.Raise(this, new MethodClassEventArgs(method));
                    mark |= ConeMethodClass.BeforeEach;
                }
                if(item is AfterEachAttribute) {
                    AfterEach.Raise(this, new MethodClassEventArgs(method));
                    mark |= ConeMethodClass.AfterEach;
                }
                if(item is AfterAllAttribute) {
                    AfterAll.Raise(this, new MethodClassEventArgs(method));
                    mark |= ConeMethodClass.AfterAll;
                }
            }
            if(mark != ConeMethodClass.Unintresting)
                return mark;
            
            Test.Raise(this, new MethodClassEventArgs(method));
            return ConeMethodClass.Test;
        }

        ConeMethodClass ClassifyMonadic(MethodInfo method, object[] attributes, ParameterInfo parameter) {
            if(typeof(ITestResult).IsAssignableFrom(parameter.ParameterType) 
                && attributes.Any(x => x is AfterEachAttribute)) {
                AfterEachWithResult.Raise(this, new MethodClassEventArgs(method));
                return ConeMethodClass.AfterEachWithResult;
            }
            return ConeMethodClass.Unintresting;
        }
    }
}
