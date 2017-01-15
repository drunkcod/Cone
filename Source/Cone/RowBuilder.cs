using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone
{
	public class RowTestData : IRowTestData
	{
		readonly MethodInfo method;
		readonly object[] parameters;
		string name;
		bool isPending;
		
		public RowTestData(MethodInfo method, object[] parameters) {
			this.method = method;
			this.parameters = parameters;
		}
		
		public string DisplayAs  { get { return name; } }

		public MethodInfo Method { get { return method; } }

		public object[] Parameters { get { return parameters; } }

		public bool HasResult { get { return false; } }
		public object Result { get { return null; } }

		public bool IsPending { get { return isPending; } }

		public RowTestData SetName(string name) {
			this.name = name;
			return this;
		}
		
		public RowTestData SetPending(bool isPending) {
			this.isPending = isPending;
			return this;
		}
	}
	
	public class RowBuilder<T> : IEnumerable<IRowTestData>
	{
		readonly IConeTestNamer testNamer;
		readonly List<IRowTestData> rows = new List<IRowTestData>();
		readonly ExpressionEvaluator evaluator = new ExpressionEvaluator();

		public RowBuilder(IConeTestNamer testNamer) {
			this.testNamer = testNamer;	
		}

		public RowBuilder<T> Add(Expression<Action<T>> testCase) =>
			AddRow(testCase, row => { });

		public RowBuilder<T> Add(string name, Expression<Action<T>> testCase) =>
			AddRow(testCase, row => row.SetName(name));


		public RowBuilder<T> Pending(Expression<Action<T>> pendingTest) =>
			AddRow(pendingTest, row => row.SetPending(true));


		public IRowTestData this[int index] { get { return rows[index]; } }

		RowBuilder<T> AddRow(Expression<Action<T>> expression, Action<RowTestData> withRow) {
			var row = CreateRow((MethodCallExpression)expression.Body); 
			withRow(row);
			rows.Add(row);
			return this;
		}

		RowTestData CreateRow(MethodCallExpression call) {
			var parameters = call.Arguments.ConvertAll(x => Collect(x, call));
			return new RowTestData(call.Method, parameters)
				.SetName(testNamer.NameFor(call.Method, parameters));
		}

		object Collect(Expression expression, Expression context) =>
			evaluator.Evaluate(expression, context, ExpressionEvaluatorParameters.Empty).Result; 

		IEnumerator<IRowTestData> IEnumerable<IRowTestData>.GetEnumerator() =>
			GetEnumerator(); 

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
			GetEnumerator(); 

		protected IEnumerator<IRowTestData> GetEnumerator() =>
			rows.GetEnumerator();
    }
}
