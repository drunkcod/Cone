using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AfterAllAttribute : Attribute
    {}
}
