﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
        class ConePadSuite : IConeSuite
        {
            class ConePadTestMethodSink : IConeTestMethodSink
            {
                readonly IConeFixture fixture;

                public ConePadTestMethodSink(IConeFixture fixture) {
                    this.fixture = fixture;
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
            readonly List<Lazy<ConePadSuite>> subsuites = new List<Lazy<ConePadSuite>>();
			readonly List<string> categories = new List<string>(); 

			Lazy<List<ConePadTest>> tests;

            public ConePadSuite(ConeFixture fixture) {
                this.fixture = fixture;
            }

            public string Name { get; set; }
			public IEnumerable<string> Categories { get { return categories; } } 
            
            public void AddSubSuite(Lazy<ConePadSuite> suite) {
                subsuites.Add(suite);
            }

            public void AddCategories(IEnumerable<string> categories) { this.categories.AddRange(categories); }
            
            ConePadTest NewTest(ConeTestNamer testNamer, MethodInfo method, object[] args, IConeAttributeProvider attributes) {
                return new ConePadTest(testNamer.TestNameFor(Name, method, args), fixture, method, args, attributes);
            }

			public void DiscoverTests(ConeTestNamer names) {
				tests = new Lazy<List<ConePadTest>>(() => {
					var foundTests = new List<ConePadTest>();
					var testSink = new ConePadTestMethodSink(fixture);
					testSink.TestFound += (method, args, attributes) => foundTests.Add(NewTest(names, method, args, attributes));
					var setup = new ConeFixtureSetup(fixture, testSink);
					setup.CollectFixtureMethods(fixture.FixtureType);
					return foundTests;
				});
			}

            public void Run(TestSession session) {
            	if(!session.IncludeSuite(this)) 
					return;
            	
				fixture.WithInitialized(
            		() => session.CollectResults(tests.Value.Cast<IConeTest>(), fixture), 
            		_ => { }, 
            		_ => { });

            	subsuites.ForEach(x => x.Value.Run(session));
            }
        }
}
