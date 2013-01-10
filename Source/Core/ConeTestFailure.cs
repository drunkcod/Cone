using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Cone.Core;
using System.Text;

namespace Cone
{
	public enum FailureType
	{
		Unknown,
		Setup,
		Test,
		Teardown
	}

	public class ConeStackFrame
	{
		public ConeStackFrame(StackFrame frame) {
			Method = frame.GetMethod();
			File = frame.GetFileName();
			Line = frame.GetFileLineNumber();
			Column = frame.GetFileColumnNumber();
		}

		public readonly MethodBase Method;
		public readonly string File;
		public readonly int Line;
		public readonly int Column;

		public override string ToString() {
			return string.Format("{0}{1}.{2}({3}) in {4}:line {5}",
				"",
				TypeFormatter.Format(Method.DeclaringType),
				Method.Name,
				string.Join(", ", Array.ConvertAll(Method.GetParameters(), Format)),
				File, Line);
		}

		string Format(ParameterInfo parameter) {
			return string.Format("{0} {1}", TypeFormatter.Format(parameter.ParameterType), parameter.Name);
		}
	}

    public class ConeTestFailure
    {
        public string File { get { return HasFrames ? StackFrames[StackFrames.Length - 1].File : null; } }
        public int Line { get { return HasFrames ? StackFrames[StackFrames.Length - 1].Line : 0; } }
        public int Column { get { return HasFrames ? StackFrames[StackFrames.Length - 1].Column : 0; } }
        public readonly string Context;
        public readonly string TestName;
        public readonly string Message;
		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;
		public readonly FailureType FailureType;
		public readonly ConeStackFrame[] StackFrames;

		bool HasFrames { get { return StackFrames.Length > 0; } }

        public ConeTestFailure(ITestName testName, Exception error, FailureType failureType) {
            TestName = testName.Name;
            Context = testName.Context;
			FailureType = failureType;

			var testError = Unwrap(error);
            var stackTrace = new StackTrace(testError, 0, true);
			var frames = stackTrace.GetFrames();
			if(frames != null)
				StackFrames = Array.ConvertAll(frames, frame => new ConeStackFrame(frame));
			else 
				StackFrames = new ConeStackFrame[0];

			Message = testError.Message;
			var expectationFailed = testError as ExpectationFailedException;
			if(expectationFailed != null) {
				Actual = expectationFailed.Actual;
				Expected = expectationFailed.Expected;
			}
        }

        public override string ToString() {
			var prefix = string.IsNullOrEmpty(File) ? string.Empty : string.Format("{0}({1}:{2}) ", File, Line, Column);
			var stackTrace = new StringBuilder();
			StackFrames.EachWhere(frame => frame.Method.DeclaringType != typeof(Verify), frame => stackTrace.AppendFormat("  at {0}\n", frame));
            return string.Format("{0}{1}.{2}: {3}\n{4}", prefix, Context, TestName, Message, stackTrace);
        }

        Exception Unwrap(Exception error) {
            var invocationException = error as TargetInvocationException;
            if (invocationException != null)
                return invocationException.InnerException;
            return error;

        }

    } 
}
