using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Cone.Samples;
using System.Xml.XPath;

namespace Cone.NUnitIntegration
{
    static class XPathNavigatorExtensions
    {
        public static int Count(this XPathNavigator navigator, string xpath) {
            return Convert.ToInt32(navigator.Evaluate("count(" + xpath + ")"));
        }
    }

    [Feature("NUnit integration", Category = "IntegrationTests")]
    public class NUnitIntegrationFeature
    {
        static readonly string SamplesPath = new Uri(typeof(ExampleFeatureFeature).Assembly.CodeBase).LocalPath;
        static string ProjectDir { 
            get {
                var binPath = Path.GetDirectoryName(SamplesPath);
                return Path.GetFullPath(Path.Combine(binPath, "..")); 
            }
        }

        [Context("when running the samples project")]
        public class Samples
        {
            static XPathNavigator SamplesResult;

            [BeforeAll]
            public void GetSamplesResult() {
                var nunitPath = Path.Combine(ProjectDir, @"Tools\NUnit-2.5.7.10213\bin\net-2.0\nunit-console.exe");

                var nunit = Process.Start(new ProcessStartInfo {
                    FileName = nunitPath,
                    Arguments = "/nologo /xmlConsole " + SamplesPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                nunit.WaitForExit();
                nunit.StandardOutput.ReadLine();
                nunit.StandardOutput.ReadLine();
                nunit.StandardOutput.ReadLine();
                SamplesResult = new XPathDocument(nunit.StandardOutput).CreateNavigator();
            }

            [Context("ExampleFeature")]
            public class ExampleFeature
            {
                public void was_executed() {
                    Verify.That(() => SamplesResult.Count("//test-suite[@type='Feature'][@name='ExampleFeature'][@executed='True']") == 1);
                }

                public void supports_nested_context() {
                    Verify.That(() => SamplesResult.Count("//test-suite[@type='Feature'][@name='ExampleFeature'][@executed='True']//test-suite[@type='Context']") == 1);
                }
            }
        }
    }
}
