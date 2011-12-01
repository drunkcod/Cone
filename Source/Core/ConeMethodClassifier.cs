using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeTestMethodSink 
    {
        void Test(MethodInfo method);
        void RowTest(MethodInfo method, IEnumerable<IRowData> rows);
        void RowSource(MethodInfo method);
    }

    public interface IConeFixtureMethodSink 
    {
        void Unintresting(MethodInfo method);
        void BeforeAll(MethodInfo method);
        void BeforeEach(MethodInfo method);
        void AfterEach(MethodInfo method);
        void AfterEachWithResult(MethodInfo method);
        void AfterAll(MethodInfo method);
    }

    public class ConeMethodClassifier 
    {
        readonly IConeFixtureMethodSink fixtureSink;
        readonly IConeTestMethodSink testSink;

        public ConeMethodClassifier(IConeFixtureMethodSink sink, IConeTestMethodSink testSink) {
            this.fixtureSink = sink;
            this.testSink = testSink;
        }

        public void Classify(MethodInfo method) {
            if(method.DeclaringType == typeof(object)) {
                fixtureSink.Unintresting(method);
                return;
            }

            if(method.Has<IRowData>(rows => testSink.RowTest(method, rows)))
                return;
           
            var parameters = method.GetParameters();
            switch(parameters.Length) {
                case 0: ClassifyNiladic(method); break;
                case 1: ClassifyMonadic(method, parameters[0]); break;
                default: fixtureSink.Unintresting(method); break;
            }
        }

        void ClassifyNiladic(MethodInfo method) {
            if(typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType)) {
                testSink.RowSource(method);
                return;
            }

            bool sunk = false;
            var attributes = method.GetCustomAttributes(true);
            for(int i = 0; i != attributes.Length; ++i) {
                var item = attributes[i];
                if(item is BeforeAllAttribute) {
                    fixtureSink.BeforeAll(method);
                    sunk = true;
                }
                if(item is BeforeEachAttribute) {
                    fixtureSink.BeforeEach(method);
                    sunk = true;
                }
                if(item is AfterEachAttribute) {
                    fixtureSink.AfterEach(method);
                    sunk = true;
                }
                if(item is AfterAllAttribute) {
                    fixtureSink.AfterAll(method);
                    sunk = true;
                }
            }
            if(sunk)
                return;
            
            testSink.Test(method);
        }

        void ClassifyMonadic(MethodInfo method, ParameterInfo parameter) {
            if(typeof(ITestResult).IsAssignableFrom(parameter.ParameterType) 
                && method.Has<AfterEachAttribute>()) {
                fixtureSink.AfterEachWithResult(method);
            }
            else fixtureSink.Unintresting(method);
        }
    }
}
