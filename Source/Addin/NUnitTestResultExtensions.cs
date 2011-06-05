using System;
using System.Diagnostics;
using NUnit.Core;

namespace Cone.Addin
{
    static class NUnitTestResultExtensions
    {
        public static TestResult Timed(this TestResult self, Action<TestResult> action, Action<TestResult> @finally) {
            var time = Stopwatch.StartNew();
            try {
                action(self);
            } finally {
                self.Time = time.Elapsed.TotalSeconds;
                @finally(self);
            }
            return self;
        }
    }
}
