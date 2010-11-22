using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Cone
{
    class ArrayExpressionStringBuilder<T>
    {
        readonly IFormatter<T> formatter;

        public ArrayExpressionStringBuilder(IFormatter<T> formatter) {
            this.formatter = formatter;
        }

        public string Format(IEnumerable<T> collection) {
            var result = new StringBuilder("new[] {");
            var format = " {0}";
            foreach (var item in collection) {
                result.AppendFormat(format, Format(item));
                format = ", {0}";
            }
            result.Append(" }");
            return result.ToString();
        }

        string Format(T value){ return formatter.Format(value); }
    }

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
            var arrayFormatter = new ArrayExpressionStringBuilder<object>(this);
            return arrayFormatter.Format(AsTyped(collection));
        }

        IEnumerable<object> AsTyped(IEnumerable collection) {
            foreach(var item in collection)
                yield return item;
        }
    }
}
