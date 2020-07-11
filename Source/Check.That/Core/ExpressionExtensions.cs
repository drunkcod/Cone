using System;
using System.Linq.Expressions;

namespace Cone.Core
{
    public static class ExpressionExtensions
    {
        public static T Execute<T>(this Expression<Func<T>> expression) => expression.Compile()();
        public static T Execute<T>(this Expression body) => Execute(Expression.Lambda<Func<T>>(body));
        public static void Execute(this Expression<Action> expression) => expression.Compile()();

        public static Expression Box(this Expression self) {
            if(self.Type.IsValueType)
                return self.Convert(typeof(object));
            return self;
        }

        public static Expression CastTo<T>(this Expression expression) {
            if(expression.Type == typeof(T))
                return expression;
            return Expression.TypeAs(expression, typeof(T)); 
        }

        public static Expression Convert(this Expression self, Type type) {
            return Expression.Convert(self, type);
        }
    }
}
