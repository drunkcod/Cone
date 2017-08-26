using System;
using System.Reflection;

namespace Cone.Core
{
	public struct Invokable
	{
		readonly MethodInfo method;
		readonly Delegate awaitAction;

		public Invokable(MethodInfo method) {
			this.method = method;
			MethodInfo wait;
			if(TryGetWait(method.ReturnType, out wait))
				awaitAction = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(method.ReturnType), null, wait);
			else awaitAction = null;
		}

		internal MethodInfo Target => method;
		public Type ReturnType => method.ReturnType;
		public string Name => method.Name;
		public bool IsWaitable => awaitAction != null;

		public object[] GetCustomAttributes(bool inherit) =>
			method.GetCustomAttributes(inherit);

		public ParameterInfo[] GetParameters() =>
			method.GetParameters();

		public object Invoke(object target, object[] args) =>
			method.Invoke(target, args);

		public object Await(object target, object[] args) {
			var r = Invoke(target, args);
			awaitAction?.DynamicInvoke(r);
			return r;
		}

		static bool TryGetWait(Type type, out MethodInfo wait) {
			wait = type.GetMethod("Wait", Type.EmptyTypes);
			return wait != null;
		}

		public static void Await(object obj) {
			MethodInfo wait;
			if(obj == null || !TryGetWait(obj.GetType(), out wait))
				return;
			((Action)Delegate.CreateDelegate(typeof(Action), obj, wait))();
		}
	}
}