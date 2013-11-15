using System;
using System.Linq.Expressions;

namespace Cone.Core
{
    [Describe(typeof(ExpressionExtensions))]
    public class ExpressionExtensionsSpec
    {
        public void ignore_redundant_casts() {
            var expression = Expression.Constant("Hello World");

            Check.That(() => Object.ReferenceEquals(expression, expression.CastTo<string>()));
        }
    }
}
