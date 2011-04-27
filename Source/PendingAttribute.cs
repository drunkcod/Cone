using System;

namespace Cone
{
    public interface IPendingAttribute 
    {
        bool IsPending { get; }
        string Reason { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PendingAttribute : Attribute, IPendingAttribute
    {
        bool IPendingAttribute.IsPending { get { return true; } }

        public string Reason { get; set; }
    }
}
