using System;
using CheckThat.Internals;

namespace CheckThat.Formatting
{
	public class TypeFormatter 
	{
		public static string Format(Type type) {
			if(type == null)
				throw new ArgumentNullException("type");

			switch(type.FullName) {
				case "System.Object": return "object";
				case "System.String": return "string";
				case "System.Boolean": return "bool";
				case "System.Int32": return "int";
				case "System.Int64": return "long";
				case "System.Single": return "float";
				case "System.Double": return "double";
				default:
					if (!type.IsGenericType) 
						return type.Name;
						
					var genArgs = type.GetGenericArguments();
					if(type.GetGenericTypeDefinition() == typeof(Nullable<>))
						return string.Format("{0}?", Format(genArgs[0]));
					
					return type.Name.Replace("`" + genArgs.Length, "<" + genArgs.ConvertAll(Format).Join(", ") + ">");
			}
		}
	}
}