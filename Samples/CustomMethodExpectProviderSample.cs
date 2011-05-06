using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;
using Moq;

namespace Cone.Samples
{
    //let's pretend we have the following interface, that we don't own.
    public interface IBrowser 
    {
        string GetLocation();
    }

    //and the following extensions
    public static class BrowserExtensions 
    {
        public static bool AtUrl(this IBrowser self, string url) {
            return self.GetLocation().EndsWith(url, true, CultureInfo.CurrentUICulture);
        }
    }

    [Feature("CustomMethodExpectProvider - how Cone does 'matchers'")]
    public class CustmomMethodExpectProviderSample
    {
        /*
            Usually this would report:
            CustomMethodExpectProvider - how Cone does 'matchers'.example:
            browser.AtUrl("/")
              Expected: true
              But was:  false
        */
        public void example() {
            var browserMock = new Mock<IBrowser>();
            browserMock.Setup(x => x.GetLocation()).Returns("http://example.com/about");
            var browser = browserMock.Object;
            Verify.That(() => browser.AtUrl("/"));
        }

        //here's one way to modify it

        //Step 1 - create a IMethodExpectProvider
        public class BrowserExtensionsMethodExpectProvider : IMethodExpectProvider
        {
            public IEnumerable<MethodInfo> GetSupportedMethods() {
                return new[]{ typeof(BrowserExtensions).GetMethod("AtUrl") };
            }

            //Step 2 - create a custom expectation
            class AtUrlExpect : StaticMethodExpect 
            {
                public AtUrlExpect(Expression body, MethodInfo method, object[] arguments): base(body, method, arguments) { }

                protected override object Actual {
                    get { 
                        return ((IBrowser)base.Actual).GetLocation();
                    }
                }

                protected override string FormatExpected(IFormatter<object> formatter) {
                    return string.Format("url ending with {0}", formatter.Format(arguments[1]));
                }
            }
            
            public IExpect GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
                return new AtUrlExpect(body, method, args);
            }
        }
    }
}
