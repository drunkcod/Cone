using System;

namespace Cone.Core
{
	public class Lazy<T>
	{
		Func<T> getValue;
 
		public Lazy(Func<T> forceValue) {
			getValue = () => {
			                 	var value = forceValue();
			                 	getValue = () => value;
			                 	return value;
			};
		}

		public T Value { get { return getValue(); } }
	}
}