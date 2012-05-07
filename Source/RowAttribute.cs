using System;
using Cone.Core;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RowAttribute : Attribute, IRowData
    {
        readonly object[] args;

        public RowAttribute(params object[] args) { this.args = args; }

        public string DisplayAs { get; set; }
        public object[] Parameters { get { return args; } } 
        public bool IsPending { get; set; }
    }
}
