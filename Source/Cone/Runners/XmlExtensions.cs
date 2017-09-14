using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cone.Xml
{
	public static class XmlExtensions
	{
		public static void WriteAttribute(this XmlWriter xml, string name, bool value) {
			xml.WriteStartAttribute(name);
			xml.WriteValue(value);
			xml.WriteEndAttribute();
		}

		public static void WriteAttribute(this XmlWriter xml, string name, int value) {
			xml.WriteStartAttribute(name);
			xml.WriteValue(value);
			xml.WriteEndAttribute();
		}

		public static T ReadXml<T>(this string input) =>
			new StringReader(input).ReadXml<T>();

		public static T ReadXml<T>(this Stream input, Encoding encoding) =>
			new StreamReader(input, encoding).ReadXml<T>();

		public static T ReadXml<T>(this TextReader input) {
			var xml = new XmlSerializer(typeof(T));
			return (T)xml.Deserialize(input);
		}
	}
}
