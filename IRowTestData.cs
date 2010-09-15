using System.Reflection;

namespace Cone
{
    public interface IRowTestData
    {
        string Name { get; }
        MethodInfo Method { get; }
        object[] Parameters { get; }
        bool IsPending { get; }
    }
}
