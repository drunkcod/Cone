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
            var results = new TestSession(log) {
                ShowProgress = ShowProgress
            };
            results.BeginSession();
            suiteTypes
                .Select(suiteBuilder.BuildSuite)
                .ForEach(x => x.Run(results));

            results.EndSession();
            results.Report();
        }
    }
}
