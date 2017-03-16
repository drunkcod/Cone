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

		public void raise_invalid_operation_exception_if_no_ctor_found() {
			var boolDisplay = new DisplayClassAttribute(typeof(BoolDisplay));
			var e = Check.Exception<InvalidOperationException>(() => boolDisplay.DisplayFor(42, typeof(int)));
			Check.That(() => e.Message == "No constructor for Cone.BoolDisplay found that takes int");
		}
	}
}
