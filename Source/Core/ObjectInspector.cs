using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cone.Core
{
    class ObjectInspector : IFormatter<object>
    {
        readonly IFormatProvider formatProvider;

        public ObjectInspector(IFormatProvider formatProvider) {
            this.formatProvider = formatProvider;
        }

        public string Inspect(object obj) {
            if(obj is string)
                return string.Format("\"{0}\"", obj);
            var type = obj.GetType();

            if(type.IsPrimitive)
                return string.Format(formatProvider, "{0}", obj);

            var sequence = obj as IEnumerable;
            if(sequence != null) 
                return (new ArrayExpressionStringBuilder<object>() as ICollectionFormatter<object>).Format(sequence, this);

            var result = new StringBuilder("{");
            var sep = " ";
            type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(x => x.Name)
                .Each(item => {
                    object value;
                    if(!TryGetValue(obj, item, out value))
                        return;
                    if(value == obj)
                        value = "this";
                    result.AppendFormat(formatProvider, "{0}{1} = {2}", sep, item.Name, Inspect(value));
                    sep = ", ";
                });
            return result.Append(" }").ToString();
        }

        bool TryGetValue(object obj, MemberInfo member, out object value) {
            switch(member.MemberType) {
                case MemberTypes.Property:
                    value = (member as PropertyInfo).GetValue(obj, null);
                    return true;
                case MemberTypes.Field: 
                    value = (member as FieldInfo).GetValue(obj);
                    return true;
                default: 
                    value = null;
                    return false;
            }
        }

        string IFormatter<object>.Format(object expression) {
            return Inspect(expression);
        }
    }

}
