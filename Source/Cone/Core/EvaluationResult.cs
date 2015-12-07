using System;
using System.Linq.Expressions;

namespace Cone.Core
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
        readonly bool isError;

        EvaluationResult(Type resultType, object value, bool isError) {
            this.resultType = resultType;
            this.value = value;
            this.isError = isError;
        }

        public static EvaluationResult Failure(Expression expression, Exception e) { 
            return new EvaluationResult(typeof(EvaluationError), new EvaluationError{ Exception = e, Expression = expression }, true); }
        public static EvaluationResult Success(Type resultType, object result) { 
            return new EvaluationResult(resultType, result, false); }

        public Type ResultType { get { return resultType; } }

        public object Result { 
            get {
                if(isError)
                    throw Exception;
                return value;
            } 
        }

        public Exception Exception { get { return Error.Exception; } }
        public Expression Expression { get { return Error.Expression; } }

        public bool IsError { get { return isError; } }
        public bool IsNull { get { return !resultType.IsValueType && Result == null; } }

        EvaluationError Error { get { return (EvaluationError)value; } }

        public EvaluationResult Then(Func<EvaluationResult, EvaluationResult> next) {
            if(isError)
                return this;
            return next(this);
        }

        public EvaluationResult Then<T>(Func<T, EvaluationResult> next) { return Then(x => next((T)x.Result)); }
    }
}
