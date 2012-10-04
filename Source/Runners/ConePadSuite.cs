using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
        public class ConePadSuite : IConeSuite
        {
            class ConePadTestMethodSink : IConeTestMethodSink
            {
                readonly IConeFixture fixture;
				readonly ConeTestNamer names;
				readonly string context;

                public ConePadTestMethodSink(ConeTestNamer names, IConeFixture fixture, string context) {
                    this.fixture = fixture;
					this.names = names;
					this.context = context;
                }

                public Action<MethodInfo, object[], object, ITestName, IConeAttributeProvider> TestFound;

                void IConeTestMethodSink.Test(MethodInfo method) { 
					TestFound(method, null, null, NameFor(method, null) , method.AsConeAttributeProvider()); 
				}

                public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                    foreach (var item in rows) {
                        var attributes = method.AsConeAttributeProvider();
                        if(item.IsPending) {
                            attributes = new ConeAttributeProvider(method.GetCustomAttributes(true).Concat(new[]{ new PendingAttribute() }));
                        }				
                        TestFound(method, item.Parameters, item.Result, GetDisplayName(method, item), attributes);
                    }
                }

				ITestName GetDisplayName(MethodInfo method, IRowData item) {
					if(item.DisplayAs == null)
						return NameFor(method, item.Parameters);
					return new ConeTestName(context, item.DisplayAs);
				}

				ITestName NameFor(MethodInfo method, object[] parameters) {
					return names.TestNameFor(context, method, parameters);
				}

                public void RowSource(MethodInfo method) {
                    var rows = ((IEnumerable<IRowTestData>)method.Invoke(fixture.Fixture, null))
                        .GroupBy(x => x.Method, x => x as IRowData);
                    foreach (var item in rows)
                        RowTest(item.Key, item);
                }
            }

            readonly ConeFixture fixture;
            readonly List<Lazy<ConePadSuite>> @subsuites = new List<Lazy<ConePadSuite>>();
			readonly List<string> @categories = new List<string>();

			Lazy<List<ConePadTest>> tests;

            public ConePadSuite(ConeFixture fixture) {
                this.fixture = fixture;
            }

            public string Name { get; set; }
			public IEnumerable<string> Categories { get { return categories; } } 
			public IEnumerable<ConePadSuite> Subsuites { 
				get { 
					return subsuites.Select(x => x.Value);
				} 
			}

			public object Fixture { get { return fixture.Fixture; } }
			public int TestCount { get { return tests.Value.Count; } }

            public void AddSubSuite(Lazy<ConePadSuite> suite) {
                subsuites.Add(suite);
            }

            public void AddCategories(IEnumerable<string> categories) { this.categories.AddRange(categories); }
            
            ConePadTest NewTest(ITestName displayName, MethodInfo method, object[] args, object result, IConeAttributeProvider attributes) {
				return new ConePadTest(displayName, fixture, method, args, result, attributes);
            }

			public void DiscoverTests(ConeTestNamer names) {
				tests = new Lazy<List<ConePadTest>>(() => {
					var foundTests = new List<ConePadTest>();
					var testSink = new ConePadTestMethodSink(names, fixture, Name);
					testSink.TestFound += (method, args, result, displayName, attributes) => 
						foundTests.Add(NewTest(displayName, method, args, result, attributes));
					var setup = new ConeFixtureSetup(GetMethodClassifier(fixture, testSink));
					setup.CollectFixtureMethods(fixture.FixtureType);
					return foundTests;
				});
			}

        	protected virtual IMethodClassifier GetMethodClassifier(
				IConeFixtureMethodSink fixtureSink, 
				IConeTestMethodSink testSink) {
        		return new ConeMethodClassifier(fixtureSink, testSink);
        	}

        	public void Run(TestSession session) {
				fixture.WithInitialized(
            		x => session.CollectResults(tests.Value.Cast<IConeTest>(), x), 
            		_ => { }, 
            		_ => { });
            }
        }
}
