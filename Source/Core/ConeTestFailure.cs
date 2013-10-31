using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Cone.Core;
using System.Text;
using System.Collections.Generic;

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

			return string.Format("{0}.{1}({2}) in {3}:line {4}",
				Method.DeclaringType != null ? TypeFormatter.Format(Method.DeclaringType) : string.Empty,
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
		public readonly IEnumerable<FailedExpectation> Errors;
		public readonly FailureType FailureType;
		public readonly ConeStackFrame[] StackFrames;

		bool HasFrames { get { return StackFrames.Length > 0; } }

        public ConeTestFailure(ITestName testName, Exception error, FailureType failureType) {
            TestName = testName.Name;
            Context = testName.Context;
			FailureType = failureType;

			var testError = Unwrap(error);
			StackFrames = GetNestedStackFrames(testError)
				.Where(ShouldIncludeFrame)
				.Select(x => new ConeStackFrame(x))
				.ToArray();

			var expectationFailed = testError as ExpectationFailedException;
			if(expectationFailed != null) 
				Errors = expectationFailed.Failures;
			else
				Errors = new List<FailedExpectation> {
					new FailedExpectation(error.Message)
				};
        }

		public string Message {
			get {
				return string.Join("\n", Errors.Select(x => x.Message).ToArray());
			}
		}

		bool ShouldIncludeFrame(StackFrame frame) {
			var m = frame.GetMethod();
			return m != null && m.DeclaringType != null && m.Module.Assembly != typeof(Verify).Assembly;
		}

        public override string ToString() {
			var prefix = string.IsNullOrEmpty(File) ? string.Empty : string.Format("{0}({1}:{2}) ", File, Line, Column);
			var stackTrace = new StringBuilder();
			StackFrames.ForEach(frame => stackTrace.AppendFormat("  at {0}\n", frame));
            return string.Format("{0}{1}.{2}: {3}\n{4}", prefix, Context, TestName, Message, stackTrace);
        }

        Exception Unwrap(Exception error) {
            var invocationException = error as TargetInvocationException;
            if (invocationException != null)
                return Unwrap(invocationException.InnerException);
            return error;
        }

		IEnumerable<StackFrame> GetNestedStackFrames(Exception e) {
			if(e == null) 
				return new StackFrame[0];
			return GetNestedStackFrames(e.InnerException).Concat(new StackTrace(e, 0, true).GetFrames() ?? new StackFrame[0]);
		}

    } 
}
