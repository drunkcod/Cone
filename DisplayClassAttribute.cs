using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class DisplayClassAttribute : Attribute 
    {
        readonly Type displayClass;

        public DisplayClassAttribute(Type displayClass, params object[] parameters) {
            this.displayClass = displayClass;
        }

        public object DisplayFor(object value) {
            return displayClass.GetConstructor(new []{ value.GetType() }).Invoke(new[]{ value });
        }
    }
}
