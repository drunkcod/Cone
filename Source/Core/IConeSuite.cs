using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeSuite : IConeTestMethodSink
    {
        string Name { get; }
        void AddSubsuite(IConeSuite suite);
        void BindTo(ConeFixtureMethods setup);
        void AddCategories(IEnumerable<string> categories);
    }
}
