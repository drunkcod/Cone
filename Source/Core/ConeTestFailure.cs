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
				Method.GetParameters().Select(Format).Join(", "),
				File, Line);
		}

		static string Format(ParameterInfo parameter) {
			return string.Format("{0} {1}", TypeFormatter.Format(parameter.ParameterType), parameter.Name);
		}
	}

    public class ConeTestFailure
    {
        public string File { get { return HasFrames ? LastFrame.File : null; } }
	    public int Line { get { return HasFrames ? LastFrame.Line : 0; } }
        public int Column { get { return HasFrames ? LastFrame.Column : 0; } }
        public readonly string Context;
        public readonly string TestName;
		public readonly FailedExpectation[] Errors;
		public readonly FailureType FailureType;
		public readonly ConeStackFrame[] StackFrames;

		bool HasFrames { get { return StackFrames.Length > 0; } }
	    ConeStackFrame LastFrame { get { return StackFrames[StackFrames.Length - 1]; } }

        public ConeTestFailure(ITestName testName, Exception error, FailureType failureType) {
            TestName = testName.Name;
            Context = testName.Context;
			FailureType = failureType;

			var testError = Unwrap(error);
			StackFrames = GetNestedStackFrames(testError)
				.Where(ShouldIncludeFrame)
				.Select(x => new ConeStackFrame(x))
				.ToArray();

			var expectationFailed = testError as CheckFailed;
			if(expectationFailed != null) 
				Errors = expectationFailed.Failures.ToArray();
			else
				Errors = new [] {
					new FailedExpectation(testError.Message)
				};
        }

		public string Message {
			get {
				return Errors.Select(x => x.Message).Join("\n");
			}
		}

		static bool ShouldIncludeFrame(StackFrame frame) {
			var m = frame.GetMethod();
			return m != null && m.DeclaringType != null && m.Module.Assembly != typeof(Check).Assembly;
		}

        public override string ToString() {
			var prefix = string.IsNullOrEmpty(File) ? string.Empty : string.Format("{0}({1}:{2}) ", File, Line, Column);
			var stackTrace = new StringBuilder();
			StackFrames.ForEach(frame => stackTrace.AppendFormat("  at {0}\n", frame));
            return string.Format("{0}{1}.{2}: {3}\n{4}", prefix, Context, TestName, Message, stackTrace);
        }

        static Exception Unwrap(Exception error) {
            var invocationException = error as TargetInvocationException;
            if (invocationException != null)
                return Unwrap(invocationException.InnerException);
            return error;
        }

		static IEnumerable<StackFrame> GetNestedStackFrames(Exception e) {
			if(e == null) 
				return new StackFrame[0];
			return GetNestedStackFrames(e.InnerException).Concat(new StackTrace(e, 0, true).GetFrames() ?? new StackFrame[0]);
		}

    } 
}
