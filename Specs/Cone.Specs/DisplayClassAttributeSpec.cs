using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
	[Describe(typeof(DisplayClassAttribute))]
	public class DisplayClassAttributeSpec
	{
		public void default_is_value_based_ctor() {
			var displayClass = new DisplayClassAttribute(typeof(BoolDisplay));

			Check.That(() => displayClass.DisplayFor(true, typeof(bool)).ToString() == "true");
		}

		public void with_additional_parameters() {
			var displayClass = new DisplayClassAttribute(typeof(BoolDisplay), "T", "F");

			Check.That(() => displayClass.DisplayFor(true, typeof(bool)).ToString() == "T");
		}
	}
}
