using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone
{
    static class ObjectConverter 
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, Func<object, object>> converters = new Dictionary<KeyValuePair<Type, Type>,Func<object,object>>();
        static readonly Func<object,object> Identity = x => x;

        public static object ChangeType(object value, Type to) {
            if(value == null)
                return null;
            return GetConverter(value.GetType(), to)(value); 
        }

        static Func<object, object> GetConverter(Type from, Type to) {
            var key = Key(from, to);
            Func<object, object> converter;
            if(converters.TryGetValue(key, out converter)) 
                return converter;
            return converters[key] = CreateConverter(key.Key, key.Value);
        }

        static Func<object, object> CreateConverter(Type from, Type to) {
            if(to.IsAssignableFrom(from))
                return Identity;
            
            var input = Expression.Parameter(typeof(object), "input");
            return Expression.Lambda<Func<object, object>>(input.Convert(from).Convert(to).Box(), input).Compile();
        }

        static KeyValuePair<Type, Type> Key(Type from, Type to) {
            return new KeyValuePair<Type,Type>(from, to);
        }
    }
}
