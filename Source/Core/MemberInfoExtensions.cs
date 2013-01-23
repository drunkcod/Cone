using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Cone.Core
{
    static class MemberInfoExtensions
    {
		delegate object Getter(object source);

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
                        return InvokeGet(getMethod, target);
                    } catch(Exception e) {
                        throw new TargetInvocationException(e);
                    }
                default: throw new NotSupportedException();
            }            
        }

        static Dictionary<MethodInfo, Getter> getterCache = new Dictionary<MethodInfo, Getter>();

        static object InvokeGet(MethodInfo getMethod, object target) {
            Getter getter;
            if(!getterCache.TryGetValue(getMethod, out getter)) {
                var name = getMethod.DeclaringType.Name + "." + getMethod.Name;
                getter = getterCache[getMethod] = getMethod.CreateGetter(name);
            }
            return getter(target);
        }

        static Getter CreateGetter(this MethodInfo self, string name) {
            var targetType = self.DeclaringType;
            var getter = new DynamicMethod(name, typeof(object), new[]{ typeof(object) }, true);
			getter.GetILGenerator()
            .Ldarg(0)
			.UnboxAsCallable(targetType)
            .CallAny(self)
            .ToObject(self.ReturnType) 
            .Ret();
            return (Getter)getter.CreateDelegate(typeof(Getter));
        }
    }
}
