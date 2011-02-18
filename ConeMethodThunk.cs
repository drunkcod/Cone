using System.Reflection;
using System.Reflection.Emit;
using System;

namespace Cone
{
    delegate void Thunk(object target, object[] parameters);

    static class ThunkCompiler 
    {
        static Type[] ThunkParameters = new []{ typeof(object), typeof(object[]) };

        public static Thunk CreateThunk(MethodInfo targetMethod) {
            var thunk = new DynamicMethod(targetMethod.Name + "Thunk", null, ThunkParameters);
            var parameters = targetMethod.GetParameters();
            if(targetMethod.IsStatic) {
            } else {
            }

            return (Thunk)thunk.CreateDelegate(typeof(Thunk));
            
        }
    }

    public class ConeMethodThunk : ICustomAttributeProvider
    {
        readonly MethodInfo method;
        readonly ConeTestNamer namer;
        readonly Thunk thunk;

        public ConeMethodThunk(MethodInfo method, ConeTestNamer namer) {
            this.method = method;
            this.namer = namer;
            this.thunk = ThunkCompiler.CreateThunk(method);
        }

        public void Invoke(object fixture, object[] parameters) { thunk(fixture, parameters); }

        public string NameFor(object[] parameters) {
            return namer.NameFor(method, parameters);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) {
            return method.GetCustomAttributes(inherit);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(System.Type attributeType, bool inherit) {
            return method.GetCustomAttributes(attributeType, inherit);
        }

        bool ICustomAttributeProvider.IsDefined(System.Type attributeType, bool inherit) {
            return method.IsDefined(attributeType, inherit);
        }
    }
}
