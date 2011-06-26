using System;
using System.Collections.Generic;
using System.Linq;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContextAttribute : Attribute
    {
        public readonly string Context;
        public string Category { get; set; }

        public ContextAttribute(string context) {
            Context = context;
        }

        public IEnumerable<string> Categories {
            get {
                if(string.IsNullOrEmpty(Category))
                    return new string[0];
                return Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
            }
        }
    }
}
