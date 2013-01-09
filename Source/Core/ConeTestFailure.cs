using System;
using System.Diagnostics;
using System.Reflection;
using Cone.Core;

namespace Cone
{
	public enum FailureType
	{
		Unknown,
		Setup,
		Test,
		Teardown
	}

    public class ConeTestFailure
    {
        public readonly string File;
        public readonly int Line;
        public readonly int Column;
        public readonly string Context;
        public readonly string TestName;
        public readonly string Message;
		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;
		public readonly FailureType FailureType;

        public ConeTestFailure(ITestName testName, Exception error, FailureType failureType) {
            TestName = testName.Name;
            Context = testName.Context;
			FailureType = failureType;

			var testError = Unwrap(error);
            var stackTrace = new StackTrace(testError, 0, true);
			var errorLocation = stackTrace.GetFrame(stackTrace.FrameCount - 1);
			if(errorLocation != null) {
				File = errorLocation.GetFileName();
				Line = errorLocation.GetFileLineNumber();
				Column = errorLocation.GetFileColumnNumber();
			}
            Message = testError.Message;
			var expectationFailed = testError as ExpectationFailedException;
			if(expectationFailed != null) {
				Actual = expectationFailed.Actual;
				Expected = expectationFailed.Expected;
			}
        }

        public override string ToString() {
			var prefix = string.IsNullOrEmpty(File) ? string.Empty : string.Format("{0}({1}:{2}) ", File, Line, Column);
            return string.Format("{0}{1}.{2}: {3}", prefix, Context, TestName, Message);
        }

        Exception Unwrap(Exception error) {
            var invocationException = error as TargetInvocationException;
            if (invocationException != null)
                return invocationException.InnerException;
            return error;

        }

    } 
}
