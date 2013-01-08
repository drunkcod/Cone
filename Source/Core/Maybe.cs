using System;

namespace Cone.Core
{
	public struct Maybe<T> : IEquatable<Maybe<T>>
	{
		readonly object value;

		private Maybe(object value) {
			this.value = value ?? Maybe.DefaultTag;
		}

		public static Maybe<T> Some(T value) { return new Maybe<T>(value); }

		public static Maybe<T> None { get { return new Maybe<T>(); } }

		public bool IsSomething { get { return value != null; } }
	
		public bool IsNone { get { return value == null; } }
		
		bool IsDefault { get { return value == Maybe.DefaultTag; } }

		public T Value {
			get { 
				if(IsNone) 
					throw new InvalidOperationException();
				return IsDefault ? default(T) : (T)value;
			}
		}

		public static bool operator==(Maybe<T> left, Maybe<T> right) {
			return left.Equals(right);
		}

		public static bool operator!=(Maybe<T> left, Maybe<T> right) { 
			return !(left == right);
		}

		public override int GetHashCode() {
			return IsNone ? 0 : value.GetHashCode();
		}

		public override bool Equals(object obj) {

			return ReferenceEquals(this, obj) || (obj is Maybe<T> && this == ((Maybe<T>)obj));
		}

		public override string ToString() {
			return IsSomething ? string.Format("Some({0})", Value) : "None";
		}

		public bool Equals(Maybe<T> other) {
			return Equals(value, other.value);
		}
	}

	public static class Maybe
	{
		static internal object DefaultTag = new object();

		public static Maybe<T> Some<T>(T value) { return Maybe<T>.Some(value); }

		public static Maybe<T> ToMaybe<T>(this T self) {
			return Maybe<T>.Some(self);
		}

		public static TResult Do<T, TResult>(this Maybe<T> self, Func<T, TResult> whenSome, Func<TResult> whenNone) {
			return self.IsSomething
				? whenSome(self.Value)
				: whenNone();
		}

		public static Maybe<TResult> Map<T, TResult>(this Maybe<T> self, Func<T, TResult> projection) {
			return self.IsSomething 
				? Maybe<TResult>.Some(projection(self.Value)) 
				: Maybe<TResult>.None;
		}

		public static Maybe<TResult> Map<T, TResult>(this Maybe<T> self, Func<T, Maybe<TResult>> projection) {
			return self.IsSomething 
				? projection(self.Value) 
				: Maybe<TResult>.None;
		}

		public static Maybe<TResult> Map<T1, T2, TResult>(Maybe<T1> arg0, Maybe<T2> arg1, Func<T1,T2,TResult> projection) {
			return (arg0.IsSomething && arg1.IsSomething) 
				? Maybe<TResult>.Some(projection(arg0.Value, arg1.Value))
				: Maybe<TResult>.None;
		}
	}
}
