using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
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
			var result = new StringWriter();
			WriteTo(new TextSessionWriter(result));
			return result.ToString();
		}

		public void WriteTo(ISessionWriter writer) {
		var prefix = string.IsNullOrEmpty(File) ? string.Empty : string.Format("{0}({1}:{2}) ", File, Line, Column);
			writer.Write("{0}{1}.{2}:\n", prefix, Context, TestName);
			writer.Important("  -> {0}\n", Message);
			StackFrames.ForEach(frame => writer.Write("  at {0}\n", frame));
		}

		public static Exception Unwrap(Exception error) {
			for(;;) {
				var invocationException = error as TargetInvocationException;
				if(invocationException != null)
					error = invocationException.InnerException;

				var innerExceptionsProp = error.GetType().GetProperty("InnerExceptions", BindingFlags.Instance | BindingFlags.Public);
				if(innerExceptionsProp == null)
					return error;
				
				var innerExceptions = innerExceptionsProp.GetValue(error, null) as ICollection<Exception>;
				if(innerExceptions != null && innerExceptions.Count == 1)
					error = innerExceptions.First();
				else return error;
			}
		}

		static IEnumerable<StackFrame> GetNestedStackFrames(Exception e) {
			if(e == null) 
				return new StackFrame[0];
			return GetNestedStackFrames(e.InnerException).Concat(new StackTrace(e, 0, true).GetFrames() ?? new StackFrame[0]);
		}
	} 
}
