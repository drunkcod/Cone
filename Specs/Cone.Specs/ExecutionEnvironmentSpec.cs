using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CheckThat;

namespace Cone
{
	[Feature("Exection environment")]
	public class ExecutionEnvironmentSpec
	{
		public void working_directory_is_directory_of_first_spec_assembly() {
			Check.That(() => Environment.CurrentDirectory == Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath));
		}
	}
}
