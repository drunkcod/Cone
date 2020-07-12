using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CheckThat.Internals;
using Cone.Core;

namespace Cone
{
	public class ConeStackFrame
	{
		public ConeStackFrame(StackFrame frame) {
			Method = frame.GetMethod();
			File = frame.GetFileName();
			Line = frame.GetFileLineNumber();
			Column = frame.GetFileColumnNumber();
		}

		public readonly MethodBase Method;
		public readonly string File;
		public readonly int Line;
		public readonly int Column;

		public override string ToString() {
			return string.Format("{0}.{1}({2}) in {3}:line {4}",
				Method.DeclaringType != null ? TypeFormatter.Format(Method.DeclaringType) : string.Empty,
				Method.Name,
				Method.GetParameters().Select(Format).Join(", "),
				File, Line);
		}

		static string Format(ParameterInfo parameter) {
			return string.Format("{0} {1}", TypeFormatter.Format(parameter.ParameterType), parameter.Name);
		}
	}
}