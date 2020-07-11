using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Runners;
using CheckThat;

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
		public string File => HasFrames ? LastFrame.File : null;
		public int Line => HasFrames ? LastFrame.Line : 0;
		public int Column => HasFrames ? LastFrame.Column : 0;
		public readonly string Context;
		public readonly string TestName;
		public readonly string ErrorsContext;
		public readonly FailedExpectation[] Errors;
		public readonly FailureType FailureType;
		public readonly ConeStackFrame[] StackFrames;

		bool HasFrames => StackFrames.Length > 0;
		ConeStackFrame LastFrame => StackFrames[StackFrames.Length - 1];

		public ConeTestFailure(ITestName testName, Exception error, FailureType failureType) : this(testName, error, failureType, ShouldIncludeFrame)
		{ }

		public ConeTestFailure(ITestName testName, Exception error, FailureType failureType, Func<StackFrame, bool> includeFrame) {
			TestName = testName.Name;
			Context = testName.Context;
			FailureType = failureType;

			var testError = Unwrap(error);
			StackFrames = GetNestedStackFrames(testError)
				.Where(includeFrame)
				.Select(x => new ConeStackFrame(x))
				.ToArray();

			var expectationFailed = testError as IFailureMessage;
			if(expectationFailed != null)
			{
				ErrorsContext = expectationFailed.Context;
				Errors = expectationFailed.Failures;
			}
			else
			{
				ErrorsContext = string.Empty;
				Errors = ExceptionAsFailure(testError).ToArray();
			}
		}

		IEnumerable<FailedExpectation> ExceptionAsFailure(Exception ex) {
			var prefix = string.Empty;
			for(; ex != null; ex = ex.InnerException) { 
				yield return new FailedExpectation(prefix + ex.Message);
				prefix += "  ";
			}
		}

		public string Message => Errors.Select(x => x.Message.ToString()).Join("\n");

		static bool ShouldIncludeFrame(StackFrame frame) {
			//return true;
			var m = frame.GetMethod();
			return m != null && m.Module.Assembly != typeof(Check).Assembly;
		}

		public override string ToString() {
			var result = new StringWriter();
			WriteTo(new TextSessionWriter(result));
			return result.ToString();
		}

		public void WriteTo(ISessionWriter writer) {
		var prefix = string.IsNullOrEmpty(File) ? string.Empty : $"{File}({Line}:{Column}) ";
			writer.Write("{0}{1}.{2}:\n", prefix, Context, TestName);
			if(!string.IsNullOrEmpty(ErrorsContext)) {
				writer.Info("given {0} →\n", ErrorsContext);
			}
			foreach(var item in Errors) 
				writer.Error(item.Message);
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

		static IEnumerable<StackFrame> GetNestedStackFrames(Exception e) => 
			e == null 
			? Enumerable.Empty<StackFrame>() 
			: GetNestedStackFrames(e.InnerException).Concat(new StackTrace(e, 0, true).GetFrames() ?? Enumerable.Empty<StackFrame>());
	} 
}
