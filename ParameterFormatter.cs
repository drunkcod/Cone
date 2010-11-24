using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Cone
{
    public class ParameterFormatter : IFormatter<object>
    {
        readonly IFormatter<IEnumerable> collectionFormatter;

        public ParameterFormatter() {
            collectionFormatter = new ArrayExpressionStringBuilder<object>(this);
        }

        public string Format(object obj) {
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
            if(typeOfObj.IsEnum)
                return string.Format("{0}.{1}", typeOfObj.Name, obj);
            return obj.ToString(); 
        }

        string FormatCollection(IEnumerable collection) {
            return collectionFormatter.Format(collection);
        }
    }
}
