using System;
using System.Reflection;

namespace Cone.Core
{
    public class TimedTestContext : IConeTest, ITestContext
    {
	    readonly IConeTest inner;
	    readonly Action<IConeTest> before;
        readonly Action<IConeTest, TimeSpan> after;
	
	    public TimedTestContext(IConeTest inner, Action<IConeTest> before, Action<IConeTest, TimeSpan> after) {
		    this.inner = inner;
            this.before = before;
            this.after = after;
	    }

	    ICustomAttributeProvider IConeTest.Attributes { get { return inner.Attributes; } }

	    void IConeTest.Run(ITestResult testResult) {
		    inner.Run(testResult);
	    }

	    Action<ITestResult> ITestContext.Establish(IFixtureContext context, Action<ITestResult> next) {
		    return r => {
                before(inner);
                inner.Timed(_ => next(r), after);
		    };
	    }
    }
}
