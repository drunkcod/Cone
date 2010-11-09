using System.Reflection;
using System.Collections.Generic;

namespace Cone
{
    public interface IConeSuite
    {
        void AddTestMethod(string name, MethodInfo method);
        void AddRowTest(string name, MethodInfo method, IEnumerable<IRowData> rows);
    }
}
