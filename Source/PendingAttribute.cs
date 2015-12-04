using System;

namespace Cone
{
	public interface IPendingAttribute
	{
		bool IsPending { get; }
		bool NoExecute { get; }
		string Reason { get; }
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class PendingAttribute : Attribute, IPendingAttribute
	{
		bool IPendingAttribute.IsPending => true;

		public string Reason { get; set; }
		public bool NoExecute { get; set; }
	}
}
