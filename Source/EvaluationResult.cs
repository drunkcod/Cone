using System;
using System.Linq.Expressions;

namespace Cone
{
    public struct EvaluationResult 
    {
        class EvaluationError 
        {
            public Exception Exception;
            public Expression Expression;
        }

        object value;
        bool isError;

        public static EvaluationResult Failure(Expression expression, Exception e){ return new EvaluationResult { value = new EvaluationError{ Exception = e, Expression = expression }, isError = true }; }
        public static EvaluationResult Success(object result){ return new EvaluationResult { value = result, isError = false }; }

        public object Value { 
            get {
                if(IsError)
                    throw Exception;
                return value; 
            } 
        }

        public Exception Exception { get { return Error.Exception; } }
        public Expression Expression { get { return Error.Expression; } }

        public bool IsError { get { return isError; } }

        EvaluationError Error { get { return (EvaluationError)value; } }
    }
}
