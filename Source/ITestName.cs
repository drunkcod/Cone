using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    public interface ITestName
    {
        string Context { get; }
        string Name { get; }
        string FullName { get; }
    }
}
