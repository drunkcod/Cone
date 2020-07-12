using System;
using System.Linq.Expressions;

namespace CheckThat.Expressions
{
    public struct EvaluationResult 
    {
        class EvaluationError 
        {
            public Exception Exception;
            public Expression Expression;
        }

        readonly Type resultType;
        readonly object value;

        EvaluationResult(Type resultType, object value) {
			this.resultType = resultType;
            this.value = value;
        }

        public static EvaluationResult Failure(Expression expression, Exception e) =>
			new EvaluationResult(typeof(EvaluationError), new EvaluationError{ Exception = e, Expression = expression });

        public static EvaluationResult Success(Type resultType, object result) => 
			new EvaluationResult(resultType, result);

        public object Result => 
			IsError ? throw Exception : value;

        public Exception Exception => Error.Exception;
        public Expression Expression => Error.Expression;

        public bool IsError => value is EvaluationError;
        public bool IsNull => !resultType.IsValueType && Result == null;

        EvaluationError Error => (EvaluationError)value;

        public EvaluationResult Then(Func<EvaluationResult, EvaluationResult> next) =>
			IsError ? this : next(this);

        public EvaluationResult Then<T>(Func<T, EvaluationResult> next) => 
			Then(x => next((T)x.Result));

		public override string ToString() =>
			IsNull ? "null" : value.ToString();
	}
}
