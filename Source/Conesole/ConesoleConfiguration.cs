using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cone.Core;
using Cone.Runners;

namespace Conesole
{
	public class ConesoleConfiguration
	{	
		class ConesoleFilterConfiguration
		{
			static Regex CreatePatternRegex(string pattern) {
				return new Regex("^" + pattern
					.Replace("\\", "\\\\")
					.Replace(".", "\\.")
					.Replace("*", ".*?"));
			}
		
			readonly List<string> includedCategories = new List<string>();
			readonly List<string> excludedCategories = new List<string>();

			Predicate<IConeTest> testFilter;
			Predicate<IConeSuite> suiteFilter;

			public bool Include(IConeTest test) => CategoryCheck(test) && (testFilter == null || testFilter(test));
			public bool Include(IConeSuite suite) => suiteFilter == null || suiteFilter(suite); 
	
			public void IncludeTests(string value) {
				var suitePattern = "*";
				var parts = value.Split('.');

				if(parts.Length > 1)
					suitePattern = string.Join(".", parts, 0, parts.Length - 1);
					
				var testPatternRegex = CreatePatternRegex(suitePattern + "." + parts.Last());
				var suitePatternRegex = CreatePatternRegex(suitePattern);

				suiteFilter = suiteFilter.Or(x => suitePatternRegex.IsMatch(x.Name));
				testFilter = testFilter.Or(x => testPatternRegex.IsMatch(x.Name));
			}

			public void Categories(string value) {
				foreach(var category in value.Split(','))
					if(category.StartsWith("!"))
						excludedCategories.Add(category.Substring(1));
					else
						includedCategories.Add(category);
			}

			bool CategoryCheck(IConeEntity entity) {
				return (includedCategories.IsEmpty() || entity.Categories.Any(includedCategories.Contains)) 
					&& !entity.Categories.Any(excludedCategories.Contains);
			}
		}

		const string OptionPrefix = "--";
		static readonly Regex OptionPattern = new Regex(string.Format("^{0}(?<option>.+)=(?<value>.+$)", OptionPrefix), RegexOptions.Compiled);

		readonly ConesoleFilterConfiguration filters = new ConesoleFilterConfiguration();

		public bool IncludeTest(IConeTest test) { return filters.Include(test); }
		public bool IncludeSuite(IConeSuite suite) { return filters.Include(suite); }  

		public LoggerVerbosity Verbosity = LoggerVerbosity.Default;
		public bool IsDryRun;
		public bool XmlConsole;
		public string XmlOutput;
		public bool TeamCityOutput;
		public bool Multicore;
		public bool ShowTimings;
		public string ConfigPath;
		public string[] AssemblyPaths;
		public string[] RunList;

		public static ConesoleConfiguration Parse(params string[] args) {
			var result = new ConesoleConfiguration();
			var paths = new List<string>();
			foreach(var item in args) {
				if(!IsOption(item)) 
					paths.Add(Path.GetFullPath(item));
				else result.ParseOption(item);
			}

			result.AssemblyPaths = paths.ToArray();
			return result;
		}

		public static bool IsOption(string value) {
			return value.StartsWith(OptionPrefix);
		}

		void ParseOption(string item) {
			if(item == "--labels") {
				Verbosity = LoggerVerbosity.Labels;
				return;
			}
 
			if(item == "--timings") {
				Verbosity = LoggerVerbosity.Labels;
				ShowTimings = true;
				return;
			}

			if(item == "--test-names") {
				Verbosity = LoggerVerbosity.TestNames;
				return;
			}
			
			if(item == "--dry-run") {
				IsDryRun = true;
				return;
			}

			if(item == "--xml-console") {
				XmlConsole = true;
				return;
			}

			if(item == "--teamcity") {
				TeamCityOutput  = true;
				return;
			}

			if(item == "--multicore") {
				Multicore = true;
				return;
			}

			if(item == "--autotest")
				return;

			if(item == "--debug")
				return;
			
			var m = OptionPattern.Match(item);
			if(!m.Success)
				throw new ArgumentException("Unknown option:" + item);
			var option = m.Groups["option"].Value;
			var valueRaw =  m.Groups["value"].Value;
			if (option == "include-tests") {
				filters.IncludeTests(valueRaw);
			} else if (option == "run-list") {
				RunList = File.ReadAllLines(valueRaw);
			}
			else if (option == "categories")
			{
				filters.Categories(valueRaw);
			}
			else if (option == "xml")
			{
				XmlOutput = valueRaw;
			}
			else if (option == "config")
			{
				var fullPath = Path.GetFullPath(valueRaw);
				ConfigPath = fullPath;
			}
			else
				throw new ArgumentException("Unknown option:" + item);
		}
	}
}