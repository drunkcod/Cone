using System;
using System.Collections;
using System.Text;

namespace Cone
{
    public class ParameterFormatter : IFormatter<object>
    {
        public string Format(object obj) {
            if (obj == null)
                return "null";
            var str = obj as string;
            if (str != null)
                return String.Format("\"{0}\"", str);
            var collection = obj as IEnumerable;
            if (collection != null)
                return FormatCollection(collection);
            return obj.ToString(); 
        }

        string FormatCollection(IEnumerable collection) {
            var result = new StringBuilder("new[] {");
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
