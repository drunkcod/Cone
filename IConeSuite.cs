using System.Reflection;
using System.Collections.Generic;

namespace Cone
{
    public interface IConeSuite
    {
        string Name { get; }
        void AddTestMethod(ConeMethodThunk testThunk);
        void AddRowTest(string name, MethodInfo method, IEnumerable<IRowData> rows);
        void AddSubsuite(IConeSuite suite);
        void BindTo(ConeFixtureMethods setup);
        void AddCategories(string categories);
    }
}
