using System;
using System.Collections;

namespace Cone.Core
{
    public class ParameterFormatter : IFormatter<object>
    {
        readonly ICollectionFormatter<object> collectionFormatter;

        public ParameterFormatter() {
            collectionFormatter = new ArrayExpressionStringBuilder<object>();
        }

        public string Format(object obj) {
            return AsWritable(obj).ToString();
        }

        public object AsWritable(object obj) {
            if (obj == null)
                return "null";
            var str = obj as string;
            if (str != null)
                return String.Format("\"{0}\"", str);
            var collection = obj as IEnumerable;
            if (collection != null)
                return FormatCollection(collection);
            var type = obj as Type;
            if(type != null)
                return string.Format("typeof({0})", type.Name);
            var typeOfObj = obj.GetType();
            //if(typeOfObj.IsEnum)
            //    return string.Format("{0}.{1}", typeOfObj.Name, obj);
            if(typeOfObj == typeof(bool))
                return (bool)obj ? "true": "false";
            return obj; 
        }

        string FormatCollection(IEnumerable collection) {
            return collectionFormatter.Format(collection, this);
        }
    }
}
