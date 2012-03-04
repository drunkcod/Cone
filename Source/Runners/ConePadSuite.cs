using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;
using System.Reflection;

namespace Cone.Runners
{
        class ConePadSuite : IConeSuite
        {
            class ConePadTestMethodSink : IConeTestMethodSink
            {
                readonly IConeFixture fixture;
                readonly ConeTestNamer names;

                public ConePadTestMethodSink(IConeFixture fixture, ConeTestNamer names) {
                    this.fixture = fixture;
                    this.names = names;
                }

                public Action<MethodInfo, object[], IConeAttributeProvider> TestFound;

                void IConeTestMethodSink.Test(MethodInfo method) { TestFound(method, null, method.AsConeAttributeProvider()); }

                public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                    foreach (var item in rows) {
                        var attributes = method.AsConeAttributeProvider();
                        if(item.IsPending) {
                            attributes = new ConeAttributeProvider(method.GetCustomAttributes(true).Concat(new[]{ new PendingAttribute() }));
                        }
                        TestFound(method, item.Parameters, attributes);
                    }
                }

                public void RowSource(MethodInfo method) {
                    var rows = ((IEnumerable<IRowTestData>)method.Invoke(fixture.Fixture, null))
                        .GroupBy(x => x.Method, x => x as IRowData);
                    foreach (var item in rows)
                        RowTest(item.Key, item);
                }
            }

            readonly ConeFixture fixture;
            readonly List<ConePadTest> tests = new List<ConePadTest>();
            readonly List<ConePadSuite> subsuites = new List<ConePadSuite>();

            public ConePadSuite(ConeFixture fixture) {
                this.fixture = fixture;
            }

            public string Name { get; set; }
            
            public void AddSubSuite(ConePadSuite suite) {
                subsuites.Add(suite);
            }

            public void AddCategories(IEnumerable<string> categories) { }
            
            public void WithTestMethodSink(ConeTestNamer testNamer, Action<IConeTestMethodSink> action) {
                var testSink = new ConePadTestMethodSink(fixture, testNamer);
                testSink.TestFound += (method, args, attributes) => tests.Add(NewTest(testNamer, method, args, attributes));
                action(testSink);
            }

            ConePadTest NewTest(ConeTestNamer testNamer, MethodInfo method, object[] args, IConeAttributeProvider attributes) {
                return new ConePadTest(testNamer.TestNameFor(Name, method, args), fixture, method, args, attributes);
            }

            public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
                action(fixture);
            }

            public IEnumerable<ConePadSuite> GetRunList() {
                return subsuites.SelectMany(x => x.GetRunList()).Concat(new[]{ this });
            }

            class NullTestResult : ITestResult 
            {
                public ITestName TestName { get { return null; } }
                public TestStatus Status { get { return TestStatus.ReadyToRun; } }

                public void Success() { }
                public void Pending(string reason) { }
                public void BeforeFailure(Exception ex) { }
                public void TestFailure(Exception ex) { }
                public void AfterFailure(Exception ex) { }
            }

            public void Run(ConePadTestResults results) {
                fixture.WithInitialized(new NullTestResult(), () => {
                    var runner = new TestExecutor(fixture);              
                    tests.ForEach(item => results.BeginTest(item, result => runner.Run(item, result)));
                }, ex => { });
            }
        }
}
