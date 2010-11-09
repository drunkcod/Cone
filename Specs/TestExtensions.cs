using System.Collections.Generic;
using System.Linq;
using NUnit.Core;

namespace Cone
{
    public static class TestExtensions
    {
        public static IEnumerable<ITest> AllTests(this ITest self) {
            yield return self;
            var children = self.Tests;
            if(children != null)
                foreach(var child in children.Cast<ITest>().SelectMany(x => x.AllTests()))
                    yield return child;
        }
    }
}
