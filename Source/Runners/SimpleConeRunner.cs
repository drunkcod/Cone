using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Cone.Runners
{
    public class SimpleConeRunner
    {
        class NullSuiteBuilder : IConeSuiteBuilder<ConePadSuite>
        {
            public bool SupportedType(Type type) { return true; }

            public ConePadSuite BuildSuite(Type suiteType) {
                return null;
            }
        }

        readonly IConeSuiteBuilder<ConePadSuite>[] suiteBuilders = new IConeSuiteBuilder<ConePadSuite>[] {
			new ConePadSuiteBuilder(),
			new NUnitSuiteBuilder(),
            new NullSuiteBuilder(),
		};

        public int Workers = 1;
            
        public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) {
        	RunTests(results, assemblies.SelectMany(x => x.GetTypes()));
        }

        public void RunTests(TestSession results, IEnumerable<Type> suiteTypes) {
            var toRun = new Queue<ConePadSuite>(suiteTypes
                .Choose<Type, ConePadSuite>(TryBuildSuite)
                .Flatten(x => x.Subsuites)
                .Where(x => results.IncludeSuite(x)));

            Verify.Initialize();
            results.RunSession(collectResults => {
                ThreadStart runSuite = () => {
                    for (ConePadSuite suite; ; ) {
                        lock (toRun) {
                            if (toRun.Count == 0)
                                return;
                            suite = toRun.Dequeue();
                        }
                        collectResults(suite);
                    }
                };
                var workers = new Thread[Workers - 1];
                for (var i = 0; i != workers.Length; ++i) {
                    var worker = workers[i] = new Thread(runSuite);
                    worker.Start();
                }
                runSuite();
                workers.ForEach(x => x.Join());
            });
            results.Report();
        }

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			suite = suiteBuilders
                .First(x => x.SupportedType(input))
                .BuildSuite(input);
			return suite != null;
		}
    }
}
