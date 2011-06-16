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
                        return getMethod.CreateBoxedInvoke()(target);
                    } catch(Exception e) {
                        throw new TargetInvocationException(e);
                    }
                default: throw new NotSupportedException();
            }            
        }

        public static Func<object, object> CreateBoxedInvoke(this MethodInfo self) {
            var targetType = self.DeclaringType;
            var getter = new DynamicMethod(targetType.Name + ".Get" + self.Name, typeof(object), new[]{ typeof(object) }, true);
            var il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, targetType);
            if(targetType.IsValueType) {
                var tmp = il.DeclareLocal(targetType);
                il.Emit(OpCodes.Stloc, tmp);
                il.Emit(OpCodes.Ldloca, tmp);
            }
            il.Emit(self.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, self);
            il.Emit(OpCodes.Box, self.ReturnType);
            il.Emit(OpCodes.Ret);
            return (Func<object,object>)getter.CreateDelegate(typeof(Func<object, object>));
        }
    }
}
