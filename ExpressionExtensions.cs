using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
    static class ExpressionExtensions
    {
        public static T Execute<T>(this Expression<Func<T>> expression) { return expression.Compile()(); }
        public static void Execute(this Expression<Action> expression) { expression.Compile()(); }
    }
}
