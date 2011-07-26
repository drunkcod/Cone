using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeSuite : IConeTestMethodSink
    {
        string Name { get; }
        IConeFixtureMethodSink FixtureSink { get; }
        void AddSubsuite(IConeSuite suite);
        void AddCategories(IEnumerable<string> categories);
    }
}
