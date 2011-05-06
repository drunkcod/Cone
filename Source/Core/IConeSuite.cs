using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeSuite : ICustomAttributeProvider
    {
        string Name { get; }
        void AddTestMethod(ConeMethodThunk testThunk);
        void AddRowTest(string name, MethodInfo method, IEnumerable<IRowData> rows);
        void AddSubsuite(IConeSuite suite);
        void BindTo(ConeFixtureMethods setup);
        void AddCategories(IEnumerable<string> categories);
    }
}
