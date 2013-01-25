using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Cone.Core
{
    static class ILGeneratorExtensions
    {
		public static ILGenerator If(this ILGenerator il, bool predicate, Func<ILGenerator, ILGenerator> ifTrue, Func<ILGenerator, ILGenerator> ifFalse) {
			return (predicate ? ifTrue : ifFalse)(il);
		}

		public static ILGenerator Call(this ILGenerator il, MethodInfo method) {
			il.Emit(OpCodes.Call, method);
			return il;
		}

        public static ILGenerator CallAny(this ILGenerator il, MethodInfo method) {
            il.Emit(method.IsVirtual && !method.DeclaringType.IsValueType ? OpCodes.Callvirt : OpCodes.Call, method);
            return il;
        }

		public static ILGenerator Ldarg(this ILGenerator il, int index) {
            il.Emit(OpCodes.Ldarg, index);
            return il;
        }

		public static ILGenerator Ldfld(this ILGenerator il, FieldInfo field) {
			il.Emit(OpCodes.Ldfld, field);
			return il;
		}

		public static ILGenerator Ldsfld(this ILGenerator il, FieldInfo field) {
			il.Emit(OpCodes.Ldsfld, field);
			return il;
		}

		public static void Ret(this ILGenerator il) {
            il.Emit(OpCodes.Ret);
        }

		public static ILGenerator UnboxAny(this ILGenerator il, Type type) {
			il.Emit(OpCodes.Unbox_Any, type);
			return il;
		}

        public static ILGenerator UnboxAsCallable(this ILGenerator il, Type boxedType) {
			il.UnboxAny(boxedType);
            if(boxedType.IsValueType) {
                var tmp = il.DeclareLocal(boxedType);
                il.Emit(OpCodes.Stloc, tmp);
                il.Emit(OpCodes.Ldloca, tmp);
            }
            return il;
        }

        public static ILGenerator ToObject(this ILGenerator il, Type topOfStack) {
            if(topOfStack.IsValueType)
                il.Emit(OpCodes.Box, topOfStack);
            return il;
        }
    }
}
