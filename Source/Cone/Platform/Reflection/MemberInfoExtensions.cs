using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Cone.Reflection
{
    static class MemberInfoExtensions
    {
		delegate object Getter(object source);

		static readonly ConcurrentDictionary<MemberInfo, Getter> getterCache = new ConcurrentDictionary<MemberInfo, Getter>();

		public static object GetValue(this MemberInfo self, object target) {
            switch(self.MemberType) {
                case MemberTypes.Field:
                    return ReadField((FieldInfo)self, target);
                case MemberTypes.Property:
					var getMethod = ((PropertyInfo)self).GetGetMethod(true);
                    return InvokeGet(getMethod, target);
                default: throw new NotSupportedException();
            }            
        }

		static object ReadField(FieldInfo field, object target) {
			var getter = GetGetter(field, field.CreateGetter);
			return getter(target);
		}

        static object InvokeGet(MethodInfo getMethod, object target) {
			try {
	            var getter = GetGetter(getMethod, getMethod.CreateGetter);
				return getter(target);
			} catch(Exception e) {
				throw new TargetInvocationException(e);
			}
        }

		static Getter GetGetter(MemberInfo member, Func<string, Getter> createGetter) {
			return getterCache.GetOrAdd(member, x => createGetter(x.DeclaringType.Name + "." + x.Name));
		}

		static Getter CreateGetter(this FieldInfo self, string name) {
            return NewGetter(name, il => il
				.If(self.IsStatic, 
					x => x.Ldsfld(self), 
					x => x.Ldarg(0)
						.UnboxAny(self.DeclaringType)
						.Ldfld(self))
				.ToObject(self.FieldType) 
				.Ret());
		}

        static Getter CreateGetter(this MethodInfo self, string name) {
			return NewGetter(name, il => il
				.If(self.IsStatic, 
					x => x.Call(self), 
					x => x.Ldarg(0)
						.UnboxAsCallable(self.DeclaringType)
						.CallAny(self))
				.ToObject(self.ReturnType) 
				.Ret());
        }

		static Getter NewGetter(string name, Action<ILGenerator> buildIl) {
            var getter = new DynamicMethod(name, typeof(object), new []{ typeof(object) }, true);
			buildIl(getter.GetILGenerator());
            return (Getter)getter.CreateDelegate(typeof(Getter));
		}
    }
}
