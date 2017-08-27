using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Cone.Core
{
	public struct Invokable : ICustomAttributeProvider
	{
		static Func<object, object> TaskAwait = obj => {
			var awaiter = ((Task)obj).GetAwaiter();
			awaiter.GetResult();
			return null;
		};

		readonly MethodInfo method;
		readonly Func<object, object> awaitAction;

		public Invokable(MethodInfo method) {
			this.method = method;
			this.awaitAction = GetWaitActionOrDefault(method.ReturnType);
		}

		internal MethodInfo Target => method;
		public Type ReturnType => method.ReturnType;
		public Type DeclaringType => method.DeclaringType;
		public string Name => method.Name;
		public bool IsWaitable => awaitAction != null;
		public bool IsStatic => method.IsStatic;

		public object[] GetCustomAttributes(bool inherit) =>
			method.GetCustomAttributes(inherit);

		public object[] GetCustomAttributes(Type attributeType, bool inherit) =>
			method.GetCustomAttributes(attributeType, inherit);

		public bool IsDefined(Type attributeType, bool inherit) =>
			method.IsDefined(attributeType, inherit);

		public ParameterInfo[] GetParameters() =>
			method.GetParameters();

		public object Invoke(object target, object[] args) =>
			method.Invoke(target, args);

		public object Await(object target, object[] args) {
			var r = Invoke(target, args);
			return IsWaitable ? awaitAction(r) : r;
		}

		static Func<object, object> GetWaitActionOrDefault(Type type)
		{
			if(type == typeof(void))
				return null;

			if(type == typeof(Task))
				return TaskAwait;

			var getAwaiter = type.GetMethod("GetAwaiter", Type.EmptyTypes);
			if(getAwaiter != null) {			
				var getResult = getAwaiter.ReturnType.GetMethod("GetResult") ??
					throw new InvalidOperationException("Can't GetResult on " + getAwaiter.ReturnType);
			
				var awaitable = Expression.Parameter(typeof(object));
				var getUnboxedResult = Expression.Call(
					Expression.Call(
						Expression.Convert(awaitable, type), 
						getAwaiter), 
					getResult);
				var body = getUnboxedResult.Type == typeof(void) 
				? Expression.Block(getUnboxedResult, Expression.Constant(null))
				: getUnboxedResult.Box();
				return Expression.Lambda<Func<object, object>>(body, awaitable)
					.Compile();
			}
			return null;
		}
	
		public static void Await(object obj) =>
			GetWaitActionOrDefault(obj.GetType())?.DynamicInvoke(obj);
	}
}