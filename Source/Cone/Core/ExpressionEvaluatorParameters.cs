using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cone.Core
{
	public class ExpressionEvaluatorParameters : IEnumerable<KeyValuePair<ParameterExpression, object>>
	{
		KeyValuePair<ParameterExpression, object>[] values = new KeyValuePair<ParameterExpression, object>[0];

		public static readonly ExpressionEvaluatorParameters Empty = new ExpressionEvaluatorParameters();

		public int Count => values.Length;
		public object this[ParameterExpression parameter] => values.First(x => x.Key == parameter).Value;

		public void Add(ParameterExpression parameter, object value) {
			if(this == Empty)
				throw new InvalidOperationException();
			var n = Count;
			Array.Resize(ref values, n + 1);
			values[n] = new KeyValuePair<ParameterExpression, object>(parameter, value);
		}

		public ParameterExpression[] GetParameters() => Array.ConvertAll(values, x => x.Key);
		public ConstantExpression[] GetValues() => Array.ConvertAll(values, x => Expression.Constant(x.Value, x.Key.Type));

		IEnumerator<KeyValuePair<ParameterExpression, object>> IEnumerable<KeyValuePair<ParameterExpression, object>>.GetEnumerator() => values.Cast<KeyValuePair<ParameterExpression,object>>().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
	}
}