using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone.Runners
{
	public class NUnitSuiteBuilder : ConePadSuiteBuilder
	{
		class NUnitFixtureDescription : IFixtureDescription 
		{
			private readonly Type type;

			public NUnitFixtureDescription(Type type) {
				this.type = type;
			}

			public IEnumerable<string> Categories
			{
				get { 
					return type.GetCustomAttributes(true)
						.Select(x => new { Type = x.GetType(), Item = x })
						.Where(x => x.Type.FullName == "NUnit.Framework.CategoryAttribute")
						.Select(x => x.Type.GetProperty("Name").GetValue(x.Item, null).ToString());
				}
			}

			public string SuiteName
			{
				get { return type.Namespace; }
			}

			public string SuiteType
			{
				get { return "TestFixture"; }
			}

			public string TestName
			{
				get { return type.Name; }
			}
		}

		public override bool SupportedType(Type type)
		{
			return type.GetCustomAttributes(true)
				.Any(x => x.GetType().FullName == "NUnit.Framework.TestFixtureAttribute"); 
		}

		public override IFixtureDescription DescriptionOf(Type fixtureType)
		{
			return new NUnitFixtureDescription(fixtureType);
		}
	}
}