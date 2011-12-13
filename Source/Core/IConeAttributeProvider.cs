using System;
using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeAttributeProvider
    {
        IEnumerable<object> GetCustomAttributes(Type type);
    }

}
