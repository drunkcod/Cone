using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cone.Core
{
	class ToStringFormatter<T> : IFormatter<T>
	{
		string IFormatter<T>.Format(T expression) => expression.ToString();
	}

	public class ArrayExpressionStringBuilder<T> : ICollectionFormatter<T>
	{
		public string Format(IEnumerable<T> collection, IFormatter<T> itemFormatter) =>
			FormatCore(collection, itemFormatter);

		
		string ICollectionFormatter<T>.Format(IEnumerable collection, IFormatter<T> itemFormatter) =>
			FormatCore(collection, itemFormatter);

		
		string IFormatter<IEnumerable<T>>.Format(IEnumerable<T> collection) => FormatCore(collection, new ToStringFormatter<T>());

		string IFormatter<IEnumerable>.Format(IEnumerable collection) => FormatCore(collection, new ToStringFormatter<T>());
		
		string FormatCore(IEnumerable collection, IFormatter<T> itemFormatter) {
			var result = new StringBuilder("new [] { ");
			var sep = string.Empty;
			foreach (T item in collection) {
				result.Append(sep);
				result.Append(itemFormatter.Format(item));
				sep = ", ";
			}
			result.Append(" }");
			return result.ToString();
		}
	}
}