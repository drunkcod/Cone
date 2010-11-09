using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BeforeAllAttribute : Attribute
    {}
}
