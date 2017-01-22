using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
		readonly ConeMessageElement[] elements;

		ConeMessage(ConeMessageElement[] elements) { this.elements = elements; }

		public static readonly ConeMessage Empty = new ConeMessage(new ConeMessageElement[0]);
		public static readonly ConeMessage NewLine = new ConeMessage(new [] { ConeMessageElement.NewLine });

		public static ConeMessage Parse(string message) { 
			var parts = Array.ConvertAll(message.Split('\n'), x => new ConeMessageElement(x, string.Empty));
			return new ConeMessage(Lines(parts).ToArray());
		}

		static IEnumerable<ConeMessageElement> Lines(ConeMessageElement[] parts) {
			if(parts.Length == 0)
				yield break;
			yield return parts[0];
			for(var i = 1; i != parts.Length; ++i) {
				yield return ConeMessageElement.NewLine;
				yield return parts[i];
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
