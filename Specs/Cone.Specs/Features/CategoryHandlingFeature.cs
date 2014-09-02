using Cone.Core;
using Cone.Runners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Features
{
	[Feature("Category Handling")]
	public class CategoryHandlingFeature
	{
		[Feature("Sugar Sweet")]
		class ExampleSpec		
		{
			public void example() { }

			[Context("I'm Sugar", Category = "Sugar")]
			public class ExampleSugarSpec
			{
				public void sugar() { }

				[Context("I'm Sweet", Category = "Sweet")]
				public class ExampleSugarSweetSpec
				{
					public void sugar_sweet() { }
				}
			}
		}

		public void categories_are_lexically_inherited() { 
			var suite = new ConePadSuiteBuilder(new DefaultFixtureProvider())
				.BuildSuite(typeof(ExampleSpec));

			var result = new List<string>();
			suite.RunAll((tests, fixture) => {
				result.AddRange(tests.Select(test => test.TestName.Name + " - " + string.Join(",", test.Categories)));
			});

			Check.That(
				() => result.Contains("sugar - Sugar"),
				() => result.Contains("sugar sweet - Sugar,Sweet")
			);
		}

	}
}
