using System;
using System.Collections.Generic;

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
                if(!string.IsNullOrEmpty(Category))
                    foreach(var item in Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        yield return item.Trim();
            }
        }
    }
}
