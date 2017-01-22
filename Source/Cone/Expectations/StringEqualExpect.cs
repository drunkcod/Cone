using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
	public class StringEqualExpect : EqualExpect
	{
		const int DisplayWidth = 62;
		static readonly int Guideoffset = ExpectMessages.EqualFormat.IndexOf('{');
		
		public StringEqualExpect(BinaryExpression body, string actual, string expected) : base(body, new ExpectValue(actual), new ExpectValue(expected)) { }

		public bool IncludeGuide = true;

		public string Preamble { 
			get {
				if(ActualString.Length == ExpectedString.Length)
					return string.Format("String lengths are both {0}.", ActualString.Length);
				return string.Format("Expected string length {0} but was {1}.", ExpectedString.Length, ActualString.Length);
			} 
		}

		public static string Center(string input, int position, int width) {
			if(input.Length <= width)
				return input;
			
			var left = width / 2;
			var prefix = string.Empty;
			var postfix = string.Empty;

			var first = Math.Max(0, position - left);
			if(first > 0)
				prefix = "…";

			var end = position + 1 + left;

			if(end < input.Length)
				postfix = "…";

			var start = first + prefix.Length;
			var value = input.Substring(start, Math.Min(width - prefix.Length - postfix.Length, input.Length - start));

			return prefix + value + postfix;      
        }

		public override ConeMessage FormatMessage(IFormatter<object> formatter) {
			if(ActualValue == null)
				return ConeMessage.Parse(string.Format(ExpectMessages.EqualFormat, formatter.Format(null), formatter.Format(ExpectedString)));
			var n = ActualString.IndexOfDifference(ExpectedString);
			var displayActual = formatter.Format(Center(ActualString, n, DisplayWidth));
			var displayExpected = formatter.Format(Center(ExpectedString, n, DisplayWidth));

			var guide = IncludeGuide 
				? new[] { new ConeMessageElement(new string(' ', displayActual.IndexOfDifference(displayExpected) + Guideoffset) + '↑', "info") }
				: new ConeMessageElement[0];

			return ConeMessage.Combine(
				ConeMessage.Parse(Preamble),
				ConeMessage.Parse(string.Format(MessageFormat, displayActual, displayExpected)),
				guide);
		}

		string ActualString { get { return ActualValue.ToString(); } }
		string ExpectedString { get { return ExpectedValue.ToString(); } }
	}
}
