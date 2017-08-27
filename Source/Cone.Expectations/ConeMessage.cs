using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cone.Expectations
{
	public class ConeMessageElement
	{
		readonly string value;
		readonly string style;

		public static readonly ConeMessageElement NewLine = new ConeMessageElement("\n", string.Empty);
		public static readonly ConeMessageElement[] NoElements = new ConeMessageElement[0];

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
		readonly ConeMessageElement[] elements;

		ConeMessage(ConeMessageElement[] elements) { this.elements = elements; }

		public static readonly ConeMessage Empty = new ConeMessage(ConeMessageElement.NoElements);
		public static readonly ConeMessage NewLine = new ConeMessage(new [] { ConeMessageElement.NewLine });

		public static ConeMessage Create(params ConeMessageElement[] elements) => new ConeMessage(elements);
		public static ConeMessage Format(string format, params object[] args) => Parse(string.Format(format, args));

		public static ConeMessage Parse(string message) { 
			var parts = message.Split('\n').Select(x => new ConeMessageElement(x, string.Empty));
			return new ConeMessage(Lines(parts).ToArray());
		}

		static IEnumerable<ConeMessageElement> Lines(IEnumerable<ConeMessageElement> parts) {
			using(var item = parts.GetEnumerator()) {
				if(!item.MoveNext())
					yield break;
				yield return item.Current;
				while(item.MoveNext()) {
					yield return ConeMessageElement.NewLine;
					yield return item.Current;
				}
			}
		}

		public static ConeMessage Combine(params IEnumerable<ConeMessageElement>[] parts) =>
			new ConeMessage(parts.SelectMany(x => x).ToArray());

		public IEnumerator<ConeMessageElement> GetEnumerator() =>
			(elements as IEnumerable<ConeMessageElement>).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => string.Concat(Array.ConvertAll(elements, x => x.ToString()));
	}
}
