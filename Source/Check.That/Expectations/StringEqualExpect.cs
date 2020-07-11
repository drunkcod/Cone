using System;
using System.Linq.Expressions;
using Cone.Core;
using System.Diagnostics;

namespace Cone.Expectations
{
	public class StringEqualExpect : EqualExpect
	{
		const int DisplayWidth = 62;
		static readonly int Guideoffset = ExpectMessages.EqualFormat("{", "{").ToString().IndexOf('{');
		static readonly ConeMessageElement[] Ellipsis = new [] { new ConeMessageElement("…", "info") };
		
		public StringEqualExpect(BinaryExpression body, string actual, string expected) : base(body, new ExpectValue(actual), new ExpectValue(expected)) { }

		public bool IncludeGuide = true;

		public ConeMessage Preamble { 
			get {
				if(ActualString.Length == ExpectedString.Length)
					return ConeMessage.Format("String lengths are both {0}.", ActualString.Length);
				return ConeMessage.Format("Expected string length {0} but was {1}.", ExpectedString.Length, ActualString.Length);
			}
		}

		public static ConeMessage Center(string input, int position, int width) {
			if(input.Length <= width)
				return ConeMessage.Parse(input);
			
			var left = width / 2;
			var prefix = ConeMessageElement.NoElements;
			var postfix = ConeMessageElement.NoElements;

			var first = Math.Max(0, position - left);
			if(first > 0)
				prefix = Ellipsis;

			var end = position + 1 + left;

			if(end < input.Length)
				postfix = Ellipsis;

			var start = first + prefix.Length;
			var value = input.Substring(start, Math.Min(width - prefix.Length - postfix.Length, input.Length - start));

			return ConeMessage.Combine(prefix, new [] { new ConeMessageElement(value, string.Empty) }, postfix);
        }

		public override ConeMessage FormatMessage(IFormatter<object> formatter) {
			if(ActualValue == null)
				return ExpectMessages.EqualFormat(
					FormatNullValue(formatter), 
					Center(ExpectedString, 0, DisplayWidth));

			if(ExpectsNull)
				return ExpectMessages.EqualFormat(
					Center(ActualString, 0, DisplayWidth), 
					FormatNullValue(formatter));

			var n = ActualString.IndexOfDifference(ExpectedString);
			var displayActual = Center(ActualString, n, DisplayWidth);
			var displayExpected = Center(ExpectedString, n, DisplayWidth);

			var guide = IncludeGuide 
				? new[] {
					ConeMessageElement.NewLine,
					new ConeMessageElement(new string(' ', displayActual.ToString().IndexOfDifference(displayExpected.ToString()) + Guideoffset) + '↑', "info") 
				}
				: ConeMessageElement.NoElements;

			return ConeMessage.Combine(
				Preamble,
				ConeMessage.NewLine,
				ExpectMessages.EqualFormat(displayActual, displayExpected),
				guide);
		}

		ConeMessage FormatNullValue(IFormatter<object> formatter) => 
			ConeMessage.Create(new ConeMessageElement(formatter.Format(null), "info"));

		string ActualString { get { return ActualValue.ToString(); } }
		string ExpectedString { get { return ExpectedValue.ToString(); } }
	}
}
