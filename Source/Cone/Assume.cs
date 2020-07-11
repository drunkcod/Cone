using System;
using System.Linq.Expressions;
using CheckThat;
using Cone.Expectations;

namespace Cone
{
	public class InvalidAssumptionException : Exception, IFailureMessage
	{
		public InvalidAssumptionException(string message, CheckFailed inner) : base(message, inner) { }

		public string Context => (InnerException as CheckFailed).Context;
		public FailedExpectation[] Failures => Array.ConvertAll((InnerException as CheckFailed).Failures, x => new FailedExpectation(
			ConeMessage.Combine( new[]{ new ConeMessageElement(Message, "info") }, x.Message), 
			x.Actual, 
			x.Expected));
	}

	public static class Assume
	{
		public static void That(Expression<Func<bool>> expr) {
			try {
				Check.That(expr);
			} catch(CheckFailed e) {
				throw new InvalidAssumptionException("Invalid assumption: ", e);
			}
		}
	}
}
