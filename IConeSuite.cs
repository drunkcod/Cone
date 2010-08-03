using System.Reflection;

namespace Cone
{
    public interface IConeSuite
    {
        void AddTestMethod(MethodInfo method);
        void AddRowTest(MethodInfo method, RowAttribute[] rows);
    }
}
