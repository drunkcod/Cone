﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone
{
    public class TestExecutor
    {
        readonly Type context;
        readonly List<ITestInterceptor> interceptors = new List<ITestInterceptor>();

        public TestExecutor(IConeFixture fixture) {
            this.context = fixture.FixtureType;
            interceptors.Add(fixture);

            var fixtureInstance = fixture.Fixture;
            fixture.FixtureType.GetFields()
                .ForEachIf(
                    x => x.FieldType.Implements<ITestInterceptor>(),
                    x => interceptors.Add((ITestInterceptor)x.GetValue(fixtureInstance)));
        }

        public void Run(IConeTest test, ITestResult testResult) {
            Verify.Context = context;
            Maybe(Before, () => {
                    Maybe(() => test.Run(testResult), testResult.Success, testResult.TestFailure);
                }, testResult.BeforeFailure);
            Maybe(() => After(testResult), () => { }, testResult.AfterFailure);
        }

        void Before() {
            interceptors.ForEach(x => x.Before());
        }

        void After(ITestResult result) { 
            interceptors.ForEach(x => x.After(result));
        }

        static void Maybe(Action action, Action then, Action<Exception> fail) {
            try {
                action();
                then();
            } catch (TargetInvocationException ex) {
                fail(ex.InnerException);
            } catch (Exception ex) {
                fail(ex);
            }
        }
    }
}
