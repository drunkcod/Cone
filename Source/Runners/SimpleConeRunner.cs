using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
    public class SimpleConeRunner
    {
        readonly IConeSuiteBuilder<ConePadSuite>[] suiteBuilders = new [] {
			new ConePadSuiteBuilder(),
			new NUnitSuiteBuilder(),
		};
            
        public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) {
        	RunTests(results, assemblies.SelectMany(x => x.GetTypes()));
        }

        public void RunTests(TestSession results ,IEnumerable<Type> suiteTypes) {
            results.BeginSession();
            suiteTypes
				.Choose<Type, ConePadSuite>(TryBuildSuite)
				.SelectMany(Flatten)
				.Where(x => results.IncludeSuite(x))
                .ForEach(x => x.Run(results));
            results.Report();
			results.EndSession();
        }

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			var builder = suiteBuilders.FirstOrDefault(x => x.SupportedType(input));
			if(builder == null) {
				suite = null;
				return false;
			}
			suite = builder.BuildSuite(input);
			return suite != null;
		}

		IEnumerable<ConePadSuite> Flatten(ConePadSuite suite) {
			yield return suite;
			foreach(var item in suite.Subsuites.SelectMany(Flatten))
				yield return item;
		}
    }
}
