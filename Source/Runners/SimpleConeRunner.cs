using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
    public class SimpleConeRunner
    {
        readonly ConePadSuiteBuilder suiteBuilder = new ConePadSuiteBuilder();
            
        public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) {
			var suites = assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType).ToList();
            RunTests(results, suites);
        }

        public void RunTests(TestSession results ,IEnumerable<Type> suiteTypes) {
            results.BeginSession();
            suiteTypes
                .Select(suiteBuilder.BuildSuite)
                .ForEach(x => x.Run(results));

            results.EndSession();
            results.Report();
        }
    }
}
