using System;
using System.Collections;
using System.Text;
using CheckThat.Internals;

namespace Cone.Core
{
	public class ParameterFormatter : IFormatter<object>
	{
		readonly ICollectionFormatter<object> collectionFormatter;

		public ParameterFormatter() {
			collectionFormatter = new ArrayExpressionStringBuilder<object>();
		}

		public string Format(object obj) =>
			AsWritable(obj).ToString();

		public object AsWritable(object obj) {
			if (obj == null)
				return "null";
			var str = obj as string;
			if (str != null)
				return string.Format("\"{0}\"", str);
			var collection = obj as IEnumerable;
			if (collection != null)
				return FormatCollection(collection);
			var type = obj as Type;
			if(type != null)
				return string.Format("typeof({0})", TypeFormatter.Format(type));
			var typeOfObj = obj.GetType();
			if(typeOfObj.IsEnum)
				return WritableEnum(obj, typeOfObj);
			if(typeOfObj == typeof(bool))
				return (bool)obj ? "true": "false";
			if(typeOfObj == typeof(char))
				return $"'{obj}'";
			return obj; 
		}

		static object WritableEnum(object obj, Type typeOfObj) {
			if (typeOfObj.Has<FlagsAttribute>()) {
				var values = Enum.GetValues(typeOfObj);
				var names = Enum.GetNames(typeOfObj);

				var parts = new StringBuilder();
				var sep = "";
				var bits = Convert.ToInt64(obj);
				for (var i = 0; i != values.Length; ++i)
					if ((bits & Convert.ToInt64(values.GetValue(i))) != 0) {
						parts.AppendFormat("{0}{{0}}.{1}", sep, names[i]);
						sep = " | ";
					}
				return string.Format(parts.ToString(), typeOfObj.Name);

			}
			return string.Format("{0}.{1}", typeOfObj.Name, obj);
		}

		string FormatCollection(IEnumerable collection) =>
			collectionFormatter.Format(collection, this);
	}
}
