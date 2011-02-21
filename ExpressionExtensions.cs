using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
    public static class ExpressionExtensions
    {
        public static T Execute<T>(this Expression<Func<T>> expression) { return expression.Compile()(); }

        public static T Execute<T>(this Expression body) { return Execute<T>(Expression.Lambda<Func<T>>(body)); }

        public static void Execute(this Expression<Action> expression) { expression.Compile()(); }

        public static T ExecuteAs<T>(this Expression body) { return body.CastTo<T>().Execute<T>(); }

        public static Expression CastTo<T>(this Expression expression) {
            if(expression.Type == typeof(T))
                return expression;
            return Expression.TypeAs(expression, typeof(T)); 
        }


    }
}
