using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

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
            var arrayFormatter = new ArrayExpressionStringBuilder<object>(this);
            return arrayFormatter.Format(AsTyped(collection));
        }

        IEnumerable<object> AsTyped(IEnumerable collection) {
            foreach(var item in collection)
                yield return item;
        }
    }
}
