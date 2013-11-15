using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Core
{
	[Describe(typeof(Maybe<>))]
	public class MaybeSpec
	{
		public void defaults_to_nothing() {
			Check.That(() => new Maybe<object>().IsNone);
		}

		public void can_contain_null() {
			Check.That(() => Maybe<object>.Some(null).Value == null);
		}

		public void Map_projects_None_to_None() {
			Check.That(() => Maybe<object>.None.Map(x => 42) == Maybe<int>.None);
		}

		public void Map_projects_Something_to_Some_value() {
			Check.That(() => Maybe.Some(1).Map(x => x + 41) == Maybe.Some(42));
		}

		public void Map_collapses_nested_Maybes() {
			Check.That(() => Maybe.Some(1).Map(x => Maybe.Some(x + 41)) == Maybe.Some(42));
		}

		public void Map_supports_multiple_values() {
			Check.That(() => Maybe.Map(Maybe.Some(1), Maybe.Some(2), (x, y) => x + y) == Maybe.Some(3));
		}

		public void GetValueOrDefault_returns_default_value_for_none() {
			Check.That(() => Maybe<string>.None.GetValueOrDefault("") == "");
			Check.That(() => Maybe<string>.None.GetValueOrDefault(() => "") == "");
		}
	}
}
