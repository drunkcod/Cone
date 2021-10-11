using System;
using System.Reflection;

namespace CheckThat.Internals
{
	public static class Lambdas
	{
		public static T Unbound<T>(MethodInfo method) where T : Delegate =>
			(T)Delegate.CreateDelegate(typeof(T), null, method);

		public static new readonly Func<object, string> ToString = Unbound<Func<object, string>>(typeof(object).GetMethod(nameof(object.ToString)));
	}
}
