using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone
{
    public static class ConePad
    {
        class ConePadSuiteBuilder : ConeSuiteBuilder<ConePadSuite>
        {
            protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
                return new ConePadSuite(type) { Name = description.TestName };
            }

            protected override void AddSubSuite(ConePadSuite suite, ConePadSuite subsuite) {
                suite.AddSubSuite(subsuite);
            }
        }

        class ConePadTestResult : ITestResult
        {
            readonly TextWriter output;
            int passed;
            List<KeyValuePair<ConePadTest, Exception>> failures = new List<KeyValuePair<ConePadTest, Exception>>();

            public ConePadTestResult(TextWriter output) {
                this.output = output;
            }

            int Passed { get { return passed; } }
            int Failed { get { return failures.Count; } }
            int Total { get { return Passed + Failed; } }
            ConePadTest runningTest;

            public TestStatus Status { get { return TestStatus.Success; } }

            public void BeginTest(ConePadTest test) {
                runningTest = test;
            }

            public void Success() {
                ++passed;
                output.Write(".");
            }

            public void Pending(string reason) { }
            public void BeforeFailure(Exception ex) { output.WriteLine("Before failure {0}", ex); }
            public void TestFailure(Exception ex) {
                output.Write("F");
                failures.Add(new KeyValuePair<ConePadTest, Exception>(runningTest, ex));
            }
            public void AfterFailure(Exception ex) { output.WriteLine("After failure {0}", ex); }

            public void Report() {
                output.WriteLine("{0} testa ran. {1} Passed. {2} Failed.\n", Total, Passed, Failed);

                if(failures.Count == 0)
                    return;
                output.WriteLine("Failures:");

                for(var i = 0; i != failures.Count; ++i) {
                    var item = failures[i];
                    var ex = item.Value;
                    var invocationException = ex as TargetInvocationException;
                    if (invocationException != null)
                        ex = invocationException.InnerException;
                    output.WriteLine("  {0,2}) {1}\n      {2}:    \n     {3}\n", i + 1, item.Key.Context, item.Key.Name, ex.Message);
                }
            }
        }

        class ConePadTest : IConeTest
        {
            readonly MethodInfo method;
            readonly object[] args;
            readonly IConeFixture fixture;

            public ConePadTest(IConeFixture fixture, MethodInfo method, object[] args) {
                this.fixture = fixture;
                this.method = method;
                this.args = args;
            }

            public string Context { get; set; }
            public string Name { get; set; }

            ICustomAttributeProvider IConeTest.Attributes { get { return method; } }
            void IConeTest.Run(ITestResult result) { method.Invoke(fixture.Fixture, args); }
        }

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

                public Action<MethodInfo, object[]> Test;

                void IConeTestMethodSink.Test(MethodInfo method) { Test(method, null); }

                public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                    foreach (var item in rows)
                        Test(method, item.Parameters);
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

            public ConePadSuite(Type fixtureType) {
                this.fixture = new ConeFixture(fixtureType);
            }

            public string Name { get; set; }
            
            public void AddSubSuite(ConePadSuite suite) {
                subsuites.Add(suite);
            }

            public void AddCategories(IEnumerable<string> categories) { }
            
            public void WithTestMethodSink(ConeTestNamer testNamer, Action<IConeTestMethodSink> action) {
                var testSink = new ConePadTestMethodSink(fixture, testNamer);
                testSink.Test += (method, args) => tests.Add(new ConePadTest(fixture, method, args) { 
                    Context = Name,
                    Name = testNamer.NameFor(method, args) 
                });
                action(testSink);
            }

            public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
                action(fixture);
            }

            public void Run(ConePadTestResult results) {
                foreach(var item in subsuites)
                    item.Run(results);
                var runner = new TestExecutor(fixture);
                foreach (var item in tests) {
                    results.BeginTest(item);
                    runner.Run(item, results);
                }
            }
        }

        public static void RunTests() {
            RunTests(Assembly.GetCallingAssembly().GetTypes());
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(Console.Out, suiteTypes);
        }

        public static void RunTests(TextWriter output, IEnumerable<Type> suites) {
            output.WriteLine("Running tests!\n----------------------------------");

            var results = new ConePadTestResult(output);
            var suiteBuilder = new ConePadSuiteBuilder();
            foreach (var item in suites.Where(suiteBuilder.SupportedType))
                suiteBuilder.BuildSuite(item).Run(results);

            output.WriteLine("\nDone.\n");
            results.Report();
        }
    }

}
