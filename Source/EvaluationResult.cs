using System;

namespace Cone
{
    public struct EvaluationResult 
    {
        object value;
        bool isError;

        public static EvaluationResult Failure(Exception e){ return new EvaluationResult { value = e, isError = true }; }
        public static EvaluationResult Success(object result){ return new EvaluationResult { value = result, isError = false }; }

        public object Value { 
            get {
                if(IsError)
                    throw (Exception)value;
                return value; 
            } 
        }
        public Exception Error { get { return (Exception)value; } }
        public bool IsError { get { return isError; } }
    }
}
