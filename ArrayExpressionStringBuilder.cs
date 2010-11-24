using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cone
{
    class ArrayExpressionStringBuilder<T> : IFormatter<IEnumerable<T>>, IFormatter<IEnumerable>
    {
        readonly IFormatter<T> formatter;

        public ArrayExpressionStringBuilder(IFormatter<T> formatter) {
            this.formatter = formatter;
        }

        public string Format(IEnumerable<T> collection) { return FormatCore(collection); }

        string IFormatter<IEnumerable>.Format(IEnumerable collection) { return FormatCore(collection); }

        string Format(T value){ return formatter.Format(value); }

        string FormatCore(IEnumerable collection) {
            var result = new StringBuilder("new[] {");
            var format = " {0}";
            foreach (var item in collection) {
                result.AppendFormat(format, Format((T)item));
                format = ", {0}";
            }
            result.Append(" }");
            return result.ToString();
        }

    }
}