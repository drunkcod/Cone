using System;
using System.Collections.Generic;
using System.Linq;
using CheckThat;
using Cone.Runners;

namespace Cone.Specs.Runners
{
	[Describe(typeof(ConeTestMethodContext))]
    public class ConeTestMethodContextSpec
    {
		class MyCategories : IHaveCategories
		{
			public string[] Categories;
			IEnumerable<string> IHaveCategories.Categories => Categories;
		}

		public void extracts_categories_from_attributes() {
			Check.With(() => ConeTestMethodContext.Attributes(new[] { new MyCategories { Categories = new[] { "MyCat" } } }))
				.That(context => context.Categories.Contains("MyCat"));
		}
    }
}
