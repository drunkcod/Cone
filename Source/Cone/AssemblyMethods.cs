using System;
using System.Reflection;

namespace Cone
{
	static class AssemblyMethods
	{
		public static Type[] GetExportedTypes(Assembly assembly) => assembly.GetExportedTypes();
	}
}
