using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
    public class SimpleConeRunner
    {
        readonly ConePadSuiteBuilder suiteBuilder = new ConePadSuiteBuilder();

        public bool ShowProgress { get; set; }

        public SimpleConeRunner() {
            ShowProgress = true;
        }
            
        public void RunTests(IConeLogger log, IEnumerable<Assembly> assemblies) {
            RunTests(log, assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
        }

        public void RunTests(IConeLogger log, IEnumerable<Type> suiteTypes) {
            var results = new ConePadTestResults(log) {
                ShowProgress = ShowProgress
            };
            results.BeginTestSession();
            var suites = suiteTypes.Select(suiteBuilder.BuildSuite);
            foreach(var test in suites.SelectMany(x => x.GetRunList()))
                test.Run(results);
            results.EndTestSession();
            results.Report();
        }
    }
}
