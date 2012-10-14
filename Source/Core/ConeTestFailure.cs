using System;
using System.Diagnostics;
using System.Reflection;

namespace Cone
{
    public class ConeTestFailure
    {
        public readonly string File;
        public readonly int Line;
        public readonly int Column;
        public readonly string Context;
        public readonly string TestName;
        public readonly string Message;

        public ConeTestFailure(ITestName testName, Exception error) {
            error = Unwrap(error);
            TestName = testName.Name;
            Context = testName.Context;
            var stackTrace= new StackTrace(error, 0, true);
			var errorLocation = stackTrace.GetFrame(stackTrace.FrameCount - 1);
			if(errorLocation != null) {
				File = errorLocation.GetFileName();
				Line = errorLocation.GetFileLineNumber();
				Column = errorLocation.GetFileColumnNumber();
			}
            Message = error.Message;
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
