using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Cone.Core
{
	public class ConeTestNamer : ITestNamer
	{
		static readonly Regex NormalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);

		readonly ParameterFormatter formatter = new ParameterFormatter();

		public string NameFor(Invokable method) => new FormatString(GetBaseName(method, x => x.Heading))
			.Format(DisplayParameters(method.GetParameters()), (string x, out string r) => { r = x; return true; });

		public string GetBaseName(Invokable method, Func<DisplayAsAttribute, string> selectName) {
			var nameAttribute = method.GetCustomAttributes(typeof(DisplayAsAttribute), true);
			if(nameAttribute.Length != 0)
				return selectName(((DisplayAsAttribute)nameAttribute[0]));
			return NormalizeNamePattern.Replace(method.Name, " ");
		}

		public ITestName TestNameFor(string context, Invokable method, object[] parameters) =>
			new ConeTestName(context, NameFor(method, parameters));

		public string NameFor(Invokable method, object[] parameters) =>
			NameFor(method, parameters, new FormatString(GetBaseName(method, x => x.Name)));

		public string NameFor(Invokable method, object[] parameters, FormatString formatString) {
			if (parameters == null)
				return formatString.ToString();
			var methodParameters = method.GetParameters();
			var displayParameters = DisplayParameters(methodParameters, parameters);
			if(formatString.HasItemFormat)
				return formatString.Format(displayParameters, (string x, out string r) => {
					var n = Array.FindIndex(methodParameters, p => p.Name == x);
					if(n < 0) {
						r = null;
						return false;
					}
					r = formatter.Format(parameters[n]);
					return true;
				});
			var result = new StringBuilder(formatString.ToString())
				.Append('(');
			return FormatParameters(result, displayParameters).Append(')').ToString();
		}

		object[] DisplayParameters(ParameterInfo[] info, object[] parameters) {
			var result = new object[parameters.Length];
			for(var i = 0; i != parameters.Length; ++i) {
				var displayClassAttribute = info[i].GetCustomAttributes(typeof(DisplayClassAttribute), true);
				var p = parameters[i];
				if(displayClassAttribute.Length == 0)
					result[i] = p;
				else
					result[i] = (displayClassAttribute[0] as DisplayClassAttribute).DisplayFor(p, info[i].ParameterType);
			}
			return result;
		}

		string[] DisplayParameters(ParameterInfo[] parameters) =>
			Array.ConvertAll(parameters, x => x.Name);

		string Format(object obj) => formatter.AsWritable(obj).ToString();

		StringBuilder FormatParameters(StringBuilder target, object[] arguments) {
			if(arguments.Length == 0)
				return target;
			var result = target.Append(Format(arguments[0]));
			for(var i = 1; i != arguments.Length; ++i)
				result.Append(", ").Append(Format(arguments[i]));
			return result;
		}
	}
}
