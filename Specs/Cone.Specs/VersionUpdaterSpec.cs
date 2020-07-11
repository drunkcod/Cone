using System;
using CheckThat;

namespace Cone.Core
{
	[Describe(typeof(VersionUpdater))]
	public class VersionUpdaterSpec
	{
		private DateTime Today;

		[BeforeAll]
		public void BeforeAll() {
			Today = DateTime.Today;
		}

		string TodaysAssemblyVersion(int revision) {
			return string.Format("AssemblyVersion(\"{0}.{1}.{2}.{3}\")", Today.Year, Today.Month, Today.Day, revision);
		}

		public void inserts_todays_date() {
			Check.That(() => VersionUpdater.Update(Today, "AssemblyVersion(\"1.0.0.0\")") == TodaysAssemblyVersion(1));
		}

		public void bumps_revision_for_additional_runs_same_day() {
			Check.That(() => VersionUpdater.Update(Today, TodaysAssemblyVersion(1)) == TodaysAssemblyVersion(2));
		}

		public void handles_multi_line_input() {
			Check.That(() => VersionUpdater.Update(Today, "\n" + TodaysAssemblyVersion(1) + "\n") == "\n" + TodaysAssemblyVersion(2) + "\n");
		}

		public void includes_arbritrary_pro_and_epilogue() {
			Check.That(() => VersionUpdater.Update(Today, "[assembly: " + TodaysAssemblyVersion(1) + "];//yay!") == "[assembly: " + TodaysAssemblyVersion(2) + "];//yay!");
		}
	}
}
