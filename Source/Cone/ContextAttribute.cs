using System;
using System.Collections.Generic;
using System.Linq;

namespace Cone
{
	public interface IContextDescription
	{
		string Context { get; }
		IEnumerable<string> Categories { get; }
	}


	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ContextAttribute : Attribute, IContextDescription
	{
		protected readonly string rawContext;
		
		public virtual string Context { get { return rawContext; } }
		public string Category { get; set; }

		public ContextAttribute(string context) {
			this.rawContext = context;
		}

		public IEnumerable<string> Categories {
			get {
				if(string.IsNullOrEmpty(Category))
					return new string[0];
				return Category.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim());
			}
		}
	}
}
