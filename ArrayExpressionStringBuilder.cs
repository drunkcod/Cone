using System.Collections.Generic;
using System.Text;

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
}