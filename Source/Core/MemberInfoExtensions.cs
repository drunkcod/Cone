using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Cone.Core
{
    static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo self, object target) {
            switch(self.MemberType) {
                case MemberTypes.Field: 
                    return (self as FieldInfo).GetValue(target);
                case MemberTypes.Property:
                    var targetType = self.DeclaringType;
                    var getMethod = ((PropertyInfo)self).GetGetMethod(true);
                    if(getMethod.IsStatic || !targetType.IsValueType) 
                        return getMethod.Invoke(target, null);
                    try {
                        return getMethod.CreateBoxedInvoke(targetType.Name + ".Get" + self.Name)(target);
                    } catch(Exception e) {
                        throw new TargetInvocationException(e);
                    }
                default: throw new NotSupportedException();
            }            
        }

        public static Func<object, object> CreateBoxedInvoke(this MethodInfo self, string name) {
            var targetType = self.DeclaringType;
            var getter = new DynamicMethod(name, typeof(object), new[]{ typeof(object) }, true);
            getter.GetILGenerator()
                .Ldarg(0)
                .UnboxAsCallable(targetType)
                .CallAny(self)
                .ToObject(self.ReturnType) 
                .Ret();
            return (Func<object,object>)getter.CreateDelegate(typeof(Func<object, object>));
        }
    }
}
