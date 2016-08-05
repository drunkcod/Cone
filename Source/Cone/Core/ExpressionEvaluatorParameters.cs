using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone.Core
{
	public class ExpressionEvaluatorParameters : IEnumerable<KeyValuePair<ParameterExpression, object>>
	{
		readonly Dictionary<ParameterExpression, object> values = new Dictionary<ParameterExpression, object>();

		public static readonly ExpressionEvaluatorParameters Empty = new ExpressionEvaluatorParameters();

		public int Count => values.Count;
		public object this[ParameterExpression parameter] => values[parameter];

		public void Add(ParameterExpression parameter, object value) => values.Add(parameter, value);

		IEnumerator<KeyValuePair<ParameterExpression, object>> IEnumerable<KeyValuePair<ParameterExpression, object>>.GetEnumerator() => values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) values).GetEnumerator();
	}
}