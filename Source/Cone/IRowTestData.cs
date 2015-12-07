using System.Reflection;
using Cone.Core;

namespace Cone
{
    public interface IRowTestData : IRowData
    {
        MethodInfo Method { get; }
    }
}
