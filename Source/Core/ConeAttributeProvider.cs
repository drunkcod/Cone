using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Core
{
    public class ConeAttributeProvider : IConeAttributeProvider 
    {
        readonly IEnumerable<object> attributes;

        public ConeAttributeProvider(IEnumerable<object> attributes) {
            this.attributes = attributes;
        }

        IEnumerable<object> IConeAttributeProvider.GetCustomAttributes(Type type) {
            return attributes.Where(x => type.IsAssignableFrom(x.GetType()));
        }
    }
}
