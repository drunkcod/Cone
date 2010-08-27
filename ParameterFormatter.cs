using System;
using System.Collections;
using System.Text;

namespace Cone
{
    public class ParameterFormatter
    {
        public string Format(object obj) {
            var collection = obj as IEnumerable;
            if (collection != null)
                return FormatCollection(collection);
            return obj.ToString(); 
        }

        string FormatCollection(IEnumerable collection) {
            var result = new StringBuilder("{");
            var format = " {0}";
            foreach (var item in collection) {
                result.AppendFormat(format, item);
                format = ", {0}";
            }
            result.Append(" }");
            return result.ToString();            
        }
    }
}
