using System;
using System.Diagnostics;
using System.IO;
using System.Xml.XPath;
using Cone.Samples;

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
			static XPathNavigator samplesResult;
            static XPathNavigator SamplesResult {
				get {
					if(samplesResult == null) {
						var nunitPath = Path.Combine(ProjectDir, @"Tools\NUnit-2.5.7.10213\bin\net-2.0\nunit-console.exe");
						var nunit = Process.Start(new ProcessStartInfo {
							FileName = nunitPath,
							Arguments = "/domain=Single /process=Single /nologo /xmlConsole " + SamplesPath,
							UseShellExecute = false,
							RedirectStandardOutput = true,
							CreateNoWindow = true
						});
						var output = nunit.StandardOutput;
						output.ReadLine();
						output.ReadLine();
						output.ReadLine();
						samplesResult = new XPathDocument(output).CreateNavigator();
						nunit.WaitForExit();
					}
					return samplesResult;
				}
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

            [Context("Failures")]
            public class Failures
            {
                public void member_access_failure() {
                    var node = (XPathNavigator)Verify.That(() => SamplesResult.SelectSingleNode("//test-case[@name='Features.Failure.member access example']") != null);
                    Verify.That(() => node.Value.StartsWith("TheAnswer == 7"));
                }

                public void string_failure() {
                    var node = (XPathNavigator)Verify.That(() => SamplesResult.SelectSingleNode("//test-case[@name='Features.Failure.string example']") != null);
                    Verify.That(() => node.Value.StartsWith("\"Hello World\".Length == 3"));
                }
            }

            [Context("Pending")]
            public class Pending
            {
                public void without_reason() {
                    var node = (XPathNavigator)Verify.That(() => SamplesResult.SelectSingleNode("//test-case[@name='Cone.PendingAttribute.without reason']") != null);
                    Verify.That(() => node.Value == "");
                }
                public void for_some_reason() {
                    var node = (XPathNavigator)Verify.That(() => SamplesResult.SelectSingleNode("//test-case[@name='Cone.PendingAttribute.for some reason']") != null);
                    Verify.That(() => node.Value == "for some reason");
                }
            }
        }
    }
}
