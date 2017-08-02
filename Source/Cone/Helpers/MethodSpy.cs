using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using static Cone.Check;

namespace Cone.Helpers
{
	public class MethodSpy 
	{
		static int nextSequenceNumber;

		int sequenceNumber;
		readonly Delegate inner;

		readonly List<object[]> invocations = new List<object[]>();

		public MethodSpy(Delegate inner) {
			this.inner = inner;
		}

		public static ActionSpy<T> On<T>(ref Action<T> target) { return On(ref target, target); }
		public static ActionSpy<T> On<T>(ref Action<T> target, Action<T> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static ActionSpy<T1, T2> On<T1, T2>(ref Action<T1, T2> target) { return On(ref target, target); }
		public static ActionSpy<T1, T2> On<T1, T2>(ref Action<T1, T2> target, Action<T1, T2> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static ActionSpy<T1, T2, T3> On<T1, T2, T3>(ref Action<T1, T2, T3> target) { return On(ref target, target); }
		public static ActionSpy<T1, T2, T3> On<T1, T2, T3>(ref Action<T1, T2, T3> target, Action<T1, T2, T3> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static ActionSpy<T1, T2, T3, T4> On<T1, T2, T3, T4>(ref Action<T1, T2, T3, T4> target) { return On(ref target, target); }
		public static ActionSpy<T1, T2, T3, T4> On<T1, T2, T3, T4>(ref Action<T1, T2, T3, T4> target, Action<T1, T2, T3, T4> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static FuncSpy<T, TResult> On<T, TResult>(ref Func<T, TResult> target) { return On(ref target, target); }
		public static FuncSpy<T, TResult> On<T, TResult>(ref Func<T, TResult> target, Func<T, TResult> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static FuncSpy<T1, T2, TResult> On<T1, T2, TResult>(ref Func<T1, T2, TResult> target) { return On(ref target, target); }
		public static FuncSpy<T1, T2, TResult> On<T1, T2, TResult>(ref Func<T1, T2, TResult> target, Func<T1, T2, TResult> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static FuncSpy<T1, T2, T3, TResult> On<T1, T2, T3, TResult>(ref Func<T1, T2, T3, TResult> target) { return On(ref target, target); }
		public static FuncSpy<T1, T2, T3, TResult> On<T1, T2, T3, TResult>(ref Func<T1, T2, T3, TResult> target, Func<T1, T2, T3, TResult> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static FuncSpy<T1, T2, T3, T4, TResult> On<T1, T2, T3, T4, TResult>(ref Func<T1, T2, T3, T4, TResult> target) { return On(ref target, target); }
		public static FuncSpy<T1, T2, T3, T4, TResult> On<T1, T2, T3, T4, TResult>(ref Func<T1, T2, T3, T4, TResult> target, Func<T1, T2, T3, T4, TResult> inner) {
			var spy = For(inner);
			target = spy;
			return spy;
		}

		public static ActionSpy<T> For<T>(Action<T> inner) { return new ActionSpy<T>(inner); }
		public static ActionSpy<T1, T2> For<T1, T2>(Action<T1, T2> inner) { return new ActionSpy<T1, T2>(inner); }
		public static ActionSpy<T1, T2, T3> For<T1, T2, T3>(Action<T1, T2, T3> inner) { return new ActionSpy<T1, T2, T3>(inner); }
		public static ActionSpy<T1, T2, T3, T4> For<T1, T2, T3, T4>(Action<T1, T2, T3, T4> inner) { return new ActionSpy<T1, T2, T3, T4>(inner); }

		public static FuncSpy<T, TResult> For<T, TResult>(Func<T, TResult> inner) { return new FuncSpy<T,TResult>(inner); }
		public static FuncSpy<T1, T2, TResult> For<T1, T2, TResult>(Func<T1, T2, TResult> inner) { return new FuncSpy<T1,T2,TResult>(inner); }
		public static FuncSpy<T1, T2, T3, TResult> For<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> inner) { return new FuncSpy<T1, T2, T3, TResult>(inner); }
		public static FuncSpy<T1, T2, T3, T4, TResult> For<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> inner) { return new FuncSpy<T1, T2, T3, T4, TResult>(inner); }

		public bool HasBeenCalled { get { return sequenceNumber != 0; } }

		public bool CalledBefore(MethodSpy other) { return sequenceNumber < other.sequenceNumber; }

		protected object Called(params object[] arguments) {
			sequenceNumber = Interlocked.Increment(ref nextSequenceNumber);
			invocations.Add(arguments);
			return InplaceInvoke(inner, arguments);
		}

		protected void Then(Delegate then) {
			if (!HasBeenCalled)
				throw new InvalidOperationException("Method has not been called.");
			foreach (var args in invocations)
				InplaceInvoke(then, args);
		}

		protected void CheckInvocations(IEnumerable<LambdaExpression> checks) {
			foreach(var args in invocations)
				new CheckWith(args).That(checks);	
		}

		static object InplaceInvoke(Delegate target, object[] args) {
			try {
				return target?.DynamicInvoke(args);
			}
			catch(TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;//nonsense.
			}
		}
	}
}
