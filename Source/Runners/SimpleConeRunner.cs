using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
    public class SimpleConeRunner
    {
        readonly ConePadSuiteBuilder suiteBuilder = new ConePadSuiteBuilder();
        readonly IConeLogger log;

        public bool ShowProgress { get; set; }

        public SimpleConeRunner(IConeLogger log) {
            ShowProgress = true;
            this.log = log;
        }
            
        public void RunTests(IEnumerable<Assembly> assemblies) {
            RunTests(assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
        }

        public void RunTests(IEnumerable<Type> suiteTypes) {
            var results = new ConePadTestResults(log) {
                ShowProgress = ShowProgress
            };
            results.BeginSession();
            var suites = suiteTypes.Select(suiteBuilder.BuildSuite);
            foreach(var test in suites.SelectMany(x => x.GetRunList()))
                test.Run(results);
            results.EndSession();
            results.Report();
        }
    }
}
