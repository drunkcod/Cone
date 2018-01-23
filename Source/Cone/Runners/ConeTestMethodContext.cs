using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone.Runners
{
	public class ConeTestMethodContext : IConeAttributeProvider
	{
		public static readonly object[] NoAttributes = new object[0];
		public static readonly ConeTestMethodContext Null = new ConeTestMethodContext(ExpectedTestResult.None, new string[0], NoAttributes);

		readonly object[] attributes;
		public readonly object[] Arguments;
		public readonly IReadOnlyCollection<string> Categories;
		public readonly ExpectedTestResult ExpectedResult;

		public static ConeTestMethodContext Attributes(object[] attributes) => 
			new ConeTestMethodContext(ExpectedTestResult.None, Null.Categories, attributes);

		public ConeTestMethodContext(ExpectedTestResult result, IReadOnlyCollection<string> cats, object[] attributes) : this(null, result, cats, attributes) { }

		public ConeTestMethodContext(object[] arguments, ExpectedTestResult result, IReadOnlyCollection<string> cats, object[] attributes) {
			this.Arguments = arguments;
			this.ExpectedResult = result;
			this.Categories = cats;
			this.attributes = attributes;
		}

		public IEnumerable<object> GetCustomAttributes(Type attributeType) =>
			attributes.Where(attributeType.IsInstanceOfType);
	}
}
