﻿using System.Collections.Generic;
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
                Unintresting(method);
                return;
            }

            if(method.AsConeAttributeProvider().Has<IRowData>(rows => testSink.RowTest(method, rows)))
                return;
           
            var parameters = method.GetParameters();
            switch(parameters.Length) {
                case 0: Niladic(method); break;
                case 1: Monadic(method, parameters[0]); break;
                default: Unintresting(method); break;
            }
        }

        void Niladic(MethodInfo method) {
            if(typeof(IEnumerable<IRowTestData>).IsAssignableFrom(method.ReturnType)) {
                testSink.RowSource(method);
                return;
            }

            bool sunk = false;
            var attributes = method.GetCustomAttributes(true);
            for(int i = 0; i != attributes.Length; ++i) {
                var item = attributes[i];
                if(item is BeforeAllAttribute) {
                    BeforeAll(method);
                    sunk = true;
                }
                if(item is BeforeEachAttribute) {
                    BeforeEach(method);
                    sunk = true;
                }
                if(item is AfterEachAttribute) {
                    AfterEach(method);
                    sunk = true;
                }
                if(item is AfterAllAttribute) {
                    AfterAll(method);
                    sunk = true;
                }
            }
            if(sunk)
                return;
            
            testSink.Test(method);
        }

        void Monadic(MethodInfo method, ParameterInfo parameter) {
            if(typeof(ITestResult).IsAssignableFrom(parameter.ParameterType) 
                && method.AsConeAttributeProvider().Has<AfterEachAttribute>()) {
                AfterEachWithResult(method);
            }
            else Unintresting(method);
        }

        void BeforeAll(MethodInfo method) { fixtureSink.BeforeAll(method); }
        void BeforeEach(MethodInfo method) { fixtureSink.BeforeEach(method); }
        void AfterEach(MethodInfo method) { fixtureSink.AfterEach(method); }
        void AfterEachWithResult(MethodInfo method) { fixtureSink.AfterEachWithResult(method); }
        void AfterAll(MethodInfo method) { fixtureSink.AfterAll(method); }
        void Unintresting(MethodInfo method) { fixtureSink.Unintresting(method); }
    }
}
