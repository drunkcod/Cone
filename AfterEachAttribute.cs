using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AfterEachAttribute : Attribute
    {}
}
