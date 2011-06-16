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

    static class ILGeneratorExtensions
    {
        public static ILGenerator Ldarg(this ILGenerator il, int index) {
            il.Emit(OpCodes.Ldarg, index);
            return il;
        }

        public static void Ret(this ILGenerator il) {
            il.Emit(OpCodes.Ret);
        }

        public static ILGenerator UnboxAsCallable(this ILGenerator il, Type boxedType) {
            il.Emit(OpCodes.Unbox_Any, boxedType);
            if(boxedType.IsValueType) {
                var tmp = il.DeclareLocal(boxedType);
                il.Emit(OpCodes.Stloc, tmp);
                il.Emit(OpCodes.Ldloca, tmp);
            }
            return il;
        }

        public static ILGenerator CallAny(this ILGenerator il, MethodInfo method) {
            il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
            return il;
        }

        public static ILGenerator ToObject(this ILGenerator il, Type topOfStack) {
            if(topOfStack.IsValueType)
                il.Emit(OpCodes.Box, topOfStack);
            return il;
        }
    }
}
