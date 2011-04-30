using System;
using System.Reflection;

namespace Cone
{
    static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo self, object target) {
            switch(self.MemberType) {
                case MemberTypes.Field: 
                    return (self as FieldInfo).GetValue(target);
                case MemberTypes.Property:
                    return (self as PropertyInfo).GetValue(target, null);
                default: throw new NotSupportedException();
            }            
        }
    }
}
