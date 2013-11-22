using System;
using System.Text.RegularExpressions;

namespace Cone.Core
{
	public static class VersionUpdater 
	{
		public static string Update(DateTime today, string input) {
			return Regex.Replace(input, @"(?<start>AssemblyVersion\("")(?<version>\d+.\d+.\d+).(?<revision>\d+)(?<end>""\))", match => {
				var newVersion = string.Format("{0}.{1}.{2}", today.Year, today.Month, today.Day);
				var revision = 1;
				if(match.Groups["version"].Value == newVersion)
					revision = int.Parse(match.Groups["revision"].Value) + 1;

				return match.Groups["start"].Value 
						+ string.Format("{0}.{1}", newVersion, revision) 
						+ match.Groups["end"].Value;
			});
		}
	}
}