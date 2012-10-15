using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            
        public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) {
        	RunTests(results, assemblies.SelectMany(x => x.GetTypes()));
        }

        public void RunTests(TestSession results, IEnumerable<Type> suiteTypes) {
            var toRun = suiteTypes
                .Choose<Type, ConePadSuite>(TryBuildSuite)
                .SelectMany(Flatten)
                .Where(x => results.IncludeSuite(x));
            
            results.RunSession(
                collectResults => toRun.ForEach(x => x.Run(collectResults)));
            results.Report();
        }

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			suite = suiteBuilders
                .First(x => x.SupportedType(input))
                .BuildSuite(input);
			return suite != null;
		}

		IEnumerable<ConePadSuite> Flatten(ConePadSuite suite) {
			yield return suite;
			foreach(var item in suite.Subsuites.SelectMany(Flatten))
				yield return item;
		}
    }
}
