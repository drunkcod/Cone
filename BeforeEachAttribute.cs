using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BeforeEachAttribute : Attribute
    {}
}
