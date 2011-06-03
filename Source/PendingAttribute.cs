using System;
using System.Reflection;
using Cone.Core;

namespace Cone
{
    public interface IPendingAttribute 
    {
        bool IsPending { get; }
        string Reason { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PendingAttribute : Attribute, IPendingAttribute, ITestContext
    {
        bool IPendingAttribute.IsPending { get { return true; } }

        public string Reason { get; set; }

		Action<ITestResult> ITestContext.Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
			return result => result.Pending(Reason);
        }
    }
}
