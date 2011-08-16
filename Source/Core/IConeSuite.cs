using System;
using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeSuite
    {
        string Name { get; }
        void AddSubsuite(IConeSuite suite);
        void AddCategories(IEnumerable<string> categories);
        void WithTestMethodSink(Action<IConeTestMethodSink> action);
        void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action);
    }
}
