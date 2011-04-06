using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone
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

        public ConeMethodClass Classify(MethodInfo method) {
            if(method.DeclaringType == typeof(object))
                return ConeMethodClass.Unintresting;
            var attributes = method.GetCustomAttributes(true);

            if(attributes.Any(x => x is RowAttribute))
                return ConeMethodClass.RowTest;
            
            var parameters = method.GetParameters();
            if(parameters.Length == 0)
                return ClassifyNiladic(method, attributes);
            else if(parameters.Length == 1)
                return ClassifyMonadic(attributes, parameters[0]);
            return ConeMethodClass.Unintresting;
        }

        ConeMethodClass ClassifyNiladic(MethodInfo method, object[] attributes) {
            if(typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType))
                return ConeMethodClass.RowSource;

            var mark = ConeMethodClass.Unintresting;
            for(int i = 0; i != attributes.Length; ++i) {
                var item = attributes[i];
                if(item is BeforeAllAttribute) mark |= ConeMethodClass.BeforeAll;
                if(item is BeforeEachAttribute) mark |= ConeMethodClass.BeforeEach;
                if(item is AfterEachAttribute) mark |= ConeMethodClass.AfterEach;
                if(item is AfterAllAttribute) mark |= ConeMethodClass.AfterAll;
            }
            if(mark != ConeMethodClass.Unintresting)
                return mark;
            
            Test.Raise(this, new MethodClassEventArgs(method));
            return ConeMethodClass.Test;
        }

        ConeMethodClass ClassifyMonadic(object[] attributes, ParameterInfo parameter) {
            if(typeof(ITestResult).IsAssignableFrom(parameter.ParameterType) 
                && attributes.Any(x => x is AfterEachAttribute))
                return ConeMethodClass.AfterEachWithResult;
            return ConeMethodClass.Unintresting;
        }
    }
}
