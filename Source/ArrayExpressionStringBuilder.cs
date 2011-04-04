using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cone
{
    interface ICollectionFormatter<T> : IFormatter<IEnumerable<T>>, IFormatter<IEnumerable>
    {
        string Format(IEnumerable<T> collection, IFormatter<T> itemFormatter);
        string Format(IEnumerable collection, IFormatter<T> itemFormatter);
    }

    class ToStringFormatter<T> : IFormatter<T>
    {
        string IFormatter<T>.Format(T expression) { return expression.ToString(); }
    }

    class ArrayExpressionStringBuilder<T> : ICollectionFormatter<T>
    {
        public string Format(IEnumerable<T> collection, IFormatter<T> itemFormatter) {
            return FormatCore(collection, itemFormatter);
        }

        string ICollectionFormatter<T>.Format(IEnumerable collection, IFormatter<T> itemFormatter) {
            return FormatCore(collection, itemFormatter);
        }

        string IFormatter<IEnumerable<T>>.Format(IEnumerable<T> collection) { return FormatCore(collection, new ToStringFormatter<T>()); }

        string IFormatter<IEnumerable>.Format(IEnumerable collection) { return FormatCore(collection, new ToStringFormatter<T>()); }

        string FormatCore(IEnumerable collection, IFormatter<T> itemFormatter) {
            var result = new StringBuilder("new[] {");
            var format = " {0}";
            foreach (var item in collection) {
                result.AppendFormat(format, itemFormatter.Format((T)item));
                format = ", {0}";
            }
            result.Append(" }");
            return result.ToString();
        }
    }
}