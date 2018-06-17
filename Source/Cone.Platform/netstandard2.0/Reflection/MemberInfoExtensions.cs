using System;
using System.Reflection;

namespace Cone.Reflection
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo self, object target) => 
            Platform.NetStandard.DynamicMember.GetValue(self, target);
    }
}