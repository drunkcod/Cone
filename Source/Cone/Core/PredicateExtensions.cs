using System;

namespace Cone.Core
{
	public static class PredicateExtensions
	{
		public static Predicate<T> And<T>(this Predicate<T> self, Predicate<T> andAlso) {
			return self == null ? andAlso : x => self(x) && andAlso(x);
		}

		public static Predicate<T> Or<T>(this Predicate<T> self, Predicate<T> orElse) {
			return self ==  null ? orElse : x => self(x) || orElse(x);
		}
	}
}