using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cone
{
    class ObjectInspector 
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

            var result = new StringBuilder("{");
            var sep = " ";
            foreach(var item in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)) {
                object value;
                if(!TryGetValue(obj, item, out value))
                    continue;
                result.AppendFormat(formatProvider, "{0}{1} = {2}", sep, item.Name, value.Inspect());
                sep = ", ";
            }
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
    }

}
