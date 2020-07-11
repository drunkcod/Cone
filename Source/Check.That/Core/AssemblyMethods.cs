using System;
using System.Reflection;

namespace Cone
{
	public static class AssemblyMethods
	{
		public static Type[] GetExportedTypes(Assembly assembly) => assembly.GetExportedTypes();
	}
}
