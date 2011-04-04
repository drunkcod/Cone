using System.Reflection;

namespace Cone
{
    public interface IRowTestData : IRowData
    {
        MethodInfo Method { get; }
    }
}
