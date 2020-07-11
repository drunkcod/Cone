using System;

namespace CheckThat.Internals
{
	public struct Maybe<T> : IEquatable<Maybe<T>>
	{
		readonly object value;

		private Maybe(object value) {
			this.value = value ?? Maybe.DefaultTag;
		}

		public static Maybe<T> Some(T value) { return new Maybe<T>(value); }

		public static Maybe<T> None { get { return new Maybe<T>(); } }

		public bool IsSomething => value != null;
	
		public bool IsNone => value == null;
		
		bool IsDefault => ReferenceEquals(value, Maybe.DefaultTag);

		T RawValue => IsDefault ? default : (T)value;

		public T Value => IsNone 
			? throw new InvalidOperationException("Can't get value from 'None'")
			: RawValue;

		public T GetValueOrDefault(T defaultValue) =>
			IsNone ? defaultValue : RawValue;

		public T GetValueOrDefault(Func<T> getDefaultValue) => 
			IsNone ? getDefaultValue() : RawValue;

		public static bool operator==(Maybe<T> left, Maybe<T> right) => 
			left.Equals(right);

		public static bool operator!=(Maybe<T> left, Maybe<T> right)  => 
			!(left == right);

		public override int GetHashCode() =>
			IsNone ? 0 : value.GetHashCode();

		public override bool Equals(object obj) =>
			ReferenceEquals(this, obj) || (obj is Maybe<T> && this == ((Maybe<T>)obj));

		public override string ToString() =>
			IsSomething ? string.Format("Some({0})", Value) : "None";

		public bool Equals(Maybe<T> other) => 
			Equals(value, other.value);
	}

	public static class Maybe
	{
		static internal object DefaultTag = new object();

		public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);

		public static Maybe<T> ToMaybe<T>(this T self) => Maybe<T>.Some(self);

		public static Maybe<T> Try<T>(Func<T> getSome) {
			try {
				return getSome().ToMaybe();
			} catch {
				return Maybe<T>.None;
			}
		}

		public static TResult Do<T, TResult>(this Maybe<T> self, Func<T, TResult> whenSome, Func<TResult> whenNone) => 
			self.IsSomething
			? whenSome(self.Value)
			: whenNone();

		public static void Do<T>(this Maybe<T> self, Action<T> whenSome, Action whenNone) {
			if(self.IsSomething)
				whenSome(self.Value);
			else whenNone();
		}

		public static Maybe<TResult> Map<T, TResult>(this Maybe<T> self, Func<T, TResult> projection) =>
			self.IsSomething 
			? Maybe<TResult>.Some(projection(self.Value)) 
			: Maybe<TResult>.None;

		public static Maybe<TResult> Map<T, TResult>(this Maybe<T> self, Func<T, Maybe<TResult>> projection) =>
			self.IsSomething 
			? projection(self.Value) 
			: Maybe<TResult>.None;

		public static Maybe<TResult> Map<T1, T2, TResult>(Maybe<T1> arg0, Maybe<T2> arg1, Func<T1,T2,TResult> projection) =>
			(arg0.IsSomething && arg1.IsSomething) 
			? Maybe<TResult>.Some(projection(arg0.Value, arg1.Value))
			: Maybe<TResult>.None;
	}
}
