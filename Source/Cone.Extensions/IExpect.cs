using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Core
{
	public class ConeMessageElement
	{
		readonly string value;
		readonly string style;

		public static readonly ConeMessageElement NewLine = new ConeMessageElement("\n", string.Empty);

		public ConeMessageElement(string value, string style) { 
			this.value = value;
			this.style = style;
		}

		public string Style => style;
		public int Length => value.Length;

		public override string ToString() => value;
	}

	public class ConeMessage : IEnumerable<ConeMessageElement>
	{
		readonly ConeMessageElement[] lines;

		ConeMessage(ConeMessageElement[] lines) { this.lines = lines; }

		public static readonly ConeMessage Empty = new ConeMessage(new ConeMessageElement[0]);

		public static ConeMessage Parse(string message) => new ConeMessage(Array.ConvertAll(message.Split('\n'), x => new ConeMessageElement(x, string.Empty)));
		public static ConeMessage Combine(params IEnumerable<ConeMessageElement>[] parts) =>
			new ConeMessage(parts.SelectMany(x => x.Where(part => part != ConeMessageElement.NewLine)).ToArray());

		public IEnumerator<ConeMessageElement> GetEnumerator() {
			if(lines.Length == 0)
				yield break;
			yield return lines[0];
			for(var i = 1; i != lines.Length; ++i) { 
				yield return ConeMessageElement.NewLine;
				yield return lines[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => string.Join("\n", Array.ConvertAll(lines, x => x.ToString()));
	}
}

namespace Cone.Expectations
{
	public interface IExpect
    {
        CheckResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatActual(IFormatter<object> formatter);
        string FormatExpected(IFormatter<object> formatter);
        ConeMessage FormatMessage(IFormatter<object> formatter);
    }
}