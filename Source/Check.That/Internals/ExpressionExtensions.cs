using System;
using System.Linq.Expressions;

namespace CheckThat.Internals
{
    public static class ExpressionExtensions
    {
        public static T Execute<T>(this Expression<Func<T>> expression) => expression.Compile()();
        public static T Execute<T>(this Expression body) => Execute(Expression.Lambda<Func<T>>(body));
        public static void Execute(this Expression<Action> expression) => expression.Compile()();

        public static Expression Box(this Expression self) => 
			self.Type.IsValueType
			? self.Convert(typeof(object))
			: self;

        public static Expression CastTo<T>(this Expression expression) => 
			expression.Type == typeof(T)
			? expression
			: Expression.TypeAs(expression, typeof(T)); 

        public static Expression Convert(this Expression self, Type type) =>
			Expression.Convert(self, type);
    }
}
