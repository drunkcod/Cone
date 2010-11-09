using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(ExpressionExtensions))]
    public class ExpressionExtensionsSpec
    {
        public void ignore_redundant_casts() {
            var expression = Expression.Constant("Hello World");

            Verify.That(() => Object.ReferenceEquals(expression, expression.CastTo<string>()));
        }
    }
}
