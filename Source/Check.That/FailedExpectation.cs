using System;
using System.Linq;
using CheckThat.Expectations;
using CheckThat.Internals;

namespace CheckThat
{
	public class FailedExpectation
	{
		public FailedExpectation(string message) : this(ConeMessage.Parse(message), Maybe<object>.None, Maybe<object>.None) { }
		public FailedExpectation(ConeMessage message, Maybe<object> actual, Maybe<object> expected) {
			this.Message = message;
			this.Actual = actual;
			this.Expected = expected;
		}

		public readonly ConeMessage Message;
		public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;
	}

	public interface IFailureMessage
	{
		string Context {get; }
		FailedExpectation[] Failures { get; }
	}

	[Serializable]
	public class CheckFailed : Exception, IFailureMessage
	{
		public static Func<ConeMessage, string> FormatMessage = Lambdas.Unbound<Func<ConeMessage,string>>(typeof(ConeMessage).GetMethod(nameof(ConeMessage.ToString)));

		public CheckFailed(string context, FailedExpectation fail, Exception innerException) : this(context, new [] { fail }, innerException) { }

		public CheckFailed(string context, FailedExpectation[] fails, Exception innerException) : base(fails.Select(x => FormatMessage(x.Message)).Join("\n"), innerException) {
			this.Context = context;
			this.Failures = fails;
		}

		public static CheckFailed WithMessage(string message) => 
			new CheckFailed(string.Empty, new[]{ new FailedExpectation(message) }, null);

		public string Context { get; }
		public FailedExpectation[] Failures { get; }
    }
}
