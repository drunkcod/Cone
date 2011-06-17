using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Cone.Core
{
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
