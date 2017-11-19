using System;
using System.Reflection;

namespace Cone.Platform.NetStandard
{
    public static class DynamicMember
    {
        public static object GetValue(MemberInfo self, object target) {
			switch(self.MemberType) {
				case MemberTypes.Field: return ((FieldInfo)self).GetValue(target);
				case MemberTypes.Property: return ((PropertyInfo)self).GetValue(target);
			}
			throw new NotSupportedException($"Can't {nameof(GetValue)} from {self}");
		}
    }
}