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
            RunTests(results, assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
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
