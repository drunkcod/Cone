using System;
using System.Drawing;
using System.Reflection;

namespace CheckThat.Internals
{
	public static class Lambdas
	{
		public static T Unbound<T>(MethodInfo method) where T : Delegate =>
			(T)Delegate.CreateDelegate(typeof(T), null, method);


		public static bool TryGetProperty<T, TProp>(string propertyName, out Func<T, TProp> found) {
			var foundProperty = typeof(Type).GetProperty(propertyName);
			if (foundProperty == null || !foundProperty.CanRead) {
				found = null;
				return false;
			}

			found = Unbound<Func<T, TProp>>(foundProperty.GetGetMethod());
			return true;
		}

		public static new readonly Func<object, string> ToString = Unbound<Func<object, string>>(typeof(object).GetMethod(nameof(object.ToString)));
	}
}
