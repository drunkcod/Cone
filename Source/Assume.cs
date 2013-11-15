using System;
using System.Linq.Expressions;

namespace Cone
{
	public class InvalidAssumptionException : Exception 
	{
		public InvalidAssumptionException(string message, Exception inner) : base(message, inner) { }
	}

	public static class Assume
	{
		public static void That(Expression<Func<bool>> expr) {
			try {
				Check.That(expr);
			} catch(Exception e) {
				throw new InvalidAssumptionException("Invalid assumption detected", e);
			}
		}
	}
}
