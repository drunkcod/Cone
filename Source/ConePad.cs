using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                return new ConePadSuite(new ConeFixture(type)) { Name = description.TestName };
            }

            protected override void AddSubSuite(ConePadSuite suite, ConePadSuite subsuite) {
                suite.AddSubSuite(subsuite);
            }
        }

        class ConePadTestResults
        {
            class ConePadTestResult : ITestResult
            {
                TestStatus testStatus;

                public Exception Error;
                public TestStatus Status { get { return testStatus; } }


                void ITestResult.Success() { testStatus = TestStatus.Success; }

                void ITestResult.Pending(string reason) { testStatus = TestStatus.Pending; }
            
                void ITestResult.BeforeFailure(Exception ex) { 
                    testStatus = TestStatus.SetupFailure;
                    Error = ex;
                }

                void ITestResult.TestFailure(Exception ex) {
                    testStatus = TestStatus.Failure;
                    Error = ex;
                }
            
                void ITestResult.AfterFailure(Exception ex) {
                    testStatus = TestStatus.TeardownFailure;
                    Error = ex;
                }
            }

            readonly TextWriter output;
            readonly List<KeyValuePair<ConePadTest, Exception>> failures = new List<KeyValuePair<ConePadTest, Exception>>();
            int passed;

            public ConePadTestResults(TextWriter output) {
                this.output = output;
            }

            int Passed { get { return passed; } }
            int Failed { get { return failures.Count; } }
            int Total { get { return Passed + Failed; } }

            public void BeginTest(ConePadTest test, Action<ITestResult> collectResult) {
                var result = new ConePadTestResult();
                collectResult(result);
                switch(result.Status) {
                    case TestStatus.Success: 
                        ++passed; 
                        output.Write(".");
                        break;
                    case TestStatus.Failure:
                        failures.Add(new KeyValuePair<ConePadTest,Exception>(test, result.Error)); 
                        output.Write("F");
                        break;
                    case TestStatus.Pending:
                        output.Write("?");
                        break;
                }
            }

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
                    output.Write("  {0,2})", i + 1);
                    var context = item.Key.Context;
                    if(!string.IsNullOrEmpty(context))
                        output.Write(" {0}\n       ", context);
                    output.WriteLine(" {0}:    \n     {1}\n", item.Key.Name, ex.Message);
                }
            }
        }

        class ConePadTest : IConeTest
        {
            readonly MethodInfo method;
            readonly object[] args;
            readonly IConeFixture fixture;
            readonly IConeAttributeProvider attributes;

            public ConePadTest(IConeFixture fixture, MethodInfo method, object[] args, IConeAttributeProvider attributes) {
                this.fixture = fixture;
                this.method = method;
                this.args = args;
                this.attributes = attributes;
            }

            public string Context { get; set; }
            public string Name { get; set; }

            IConeAttributeProvider IConeTest.Attributes { get { return attributes; } }
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
                return new ConePadTest(fixture, method, args, attributes) { 
                    Context = Name,
                    Name = testNamer.NameFor(method, args),
                };
            }

            public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
                action(fixture);
            }

            public IEnumerable<ConePadSuite> GetRunList() {
                return subsuites.SelectMany(x => x.GetRunList()).Concat(new[]{ this });
            }

            public void Run(ConePadTestResults results) {
                fixture.WithInitialized(null, () => {
                    var runner = new TestExecutor(fixture);              
                    foreach (var item in tests) {
                        results.BeginTest(item, result => runner.Run(item, result));
                    }
                });
            }
        }

        public class SimpleConeRunner
        {
            readonly ConePadSuiteBuilder suiteBuilder = new ConePadSuiteBuilder();

            public void RunTests(TextWriter output, IEnumerable<Type> suiteTypes) {
                var results = new ConePadTestResults(output);
                var time = Stopwatch.StartNew();
                var suites = ConvertAll(suiteTypes.ToArray(), suiteBuilder.BuildSuite);
                var runLists = ConvertAll(suites, x => x.GetRunList());
                ConvertAll(runLists.SelectMany(x => x).ToArray(), x => { x.Run(results); return true; });

                results.Report();
                output.WriteLine("\nDone in {0}.\n", time.Elapsed);
            }

            protected virtual TOutput[] ConvertAll<TInput,TOutput>(TInput[] input, Converter<TInput, TOutput> transform) {
                return Array.ConvertAll(input, transform);
            }
        }

        public static void RunTests() {
            Verify.GetPluginAssemblies = () => new[]{ typeof(Verify).Assembly };
            RunTests(Console.Out, Assembly.GetCallingAssembly().GetTypes().Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(TextWriter output, IEnumerable<Assembly> assemblies) {
            Verify.GetPluginAssemblies = () => assemblies.Concat(new[]{ typeof(Verify).Assembly });
            RunTests(Console.Out, assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(Console.Out, suiteTypes);
        }

        public static void RunTests(TextWriter output, IEnumerable<Type> suites) {
            output.WriteLine("Running tests!\n----------------------------------");
            var runner = new SimpleConeRunner();
            runner.RunTests(output, suites);
        }
    }
}
