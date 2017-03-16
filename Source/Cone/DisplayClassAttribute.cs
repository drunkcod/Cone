using Cone.Core;
using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class DisplayClassAttribute : Attribute 
    {
        readonly Type displayClass;
        readonly object[] parameters;

        public DisplayClassAttribute(Type displayClass, params object[] parameters) {
            this.displayClass = displayClass;
            this.parameters = parameters;
        }

        public object DisplayFor(object value, Type valueType) {
            var length = Length(parameters) + 1;
            var ctorParameters = new object[length];
            Array.Copy(parameters, 0, ctorParameters, 1, length - 1);
            
            var types = new Type[length];
            for(var i = 1; i < length; ++i)
                types[i] = ctorParameters[i].GetType();

            types[0] = valueType;
            ctorParameters[0] = value;
			var ctor = displayClass.GetConstructor(types);
			if(ctor == null)
				throw new InvalidOperationException($"No constructor for {displayClass} found that takes {string.Join(", ", Array.ConvertAll(types, TypeFormatter.Format))}");
			return ctor.Invoke(ctorParameters);
        }

        int Length(object[] array) {
            if(array == null)
                return 0;
            return array.Length;
        }

    }
}
