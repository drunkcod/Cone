using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace CheckThat.Internals
{
    static class MemberInfoExtensions
    {
		delegate object Getter(object source);

		static readonly ConcurrentDictionary<MemberInfo, Getter> getterCache = new ConcurrentDictionary<MemberInfo, Getter>();

		public static object GetValue(this MemberInfo self, object target) =>
			getterCache.GetOrAdd(self, CreateGetter)(target);

		static Getter CreateGetter(MemberInfo x) {
			var input = Expression.Parameter(typeof(object));
			var getter = Expression.Lambda<Getter>(
				Expression.Convert(
					ReadMember(Expression.Convert(input, x.DeclaringType), x),
					typeof(object)),
				input);
			return getter.Compile();
		}

		static Expression ReadMember(Expression source, MemberInfo member) {
			switch (member.MemberType) {
				default: throw new NotSupportedException();
				case MemberTypes.Property:
					var prop = (PropertyInfo)member;
					return Expression.Call(prop.GetMethod.IsStatic ? null : source, prop.GetMethod);

				case MemberTypes.Field:
					var field = (FieldInfo)member;
					return Expression.Field(field.IsStatic ? null : source, field);
			}
		}
	}
}
