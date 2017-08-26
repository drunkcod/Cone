using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cone.Core
{
	public struct Invokable
	{
		static Delegate TaskAwait = Delegate.CreateDelegate(
			typeof(Action<Task>), 
			typeof(Task).GetMethod(nameof(Task.Wait), Type.EmptyTypes));

		readonly MethodInfo method;
		readonly Delegate awaitAction;

		public Invokable(MethodInfo method) {
			this.method = method;
			this.awaitAction = GetWaitActionOrDefault(method.ReturnType);
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

		static Delegate GetWaitActionOrDefault(Type type)
		{
			if(typeof(Task).IsAssignableFrom(type))
				return TaskAwait;
			MethodInfo wait;
			if(TryGetWait(type, out wait))
				return Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), null, wait);

			return null;
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