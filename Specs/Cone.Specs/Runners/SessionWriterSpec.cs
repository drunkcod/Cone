using System.IO;
using CheckThat;
using CheckThat.Expectations;

namespace Cone.Runners
{
	[Describe(typeof(ISessionWriter))]
	public class SessionWriterSpec
	{
		public void error_block_layout() { 
			var result = new StringWriter();
			var writer = new TextSessionWriter(result);
			
			writer.Error(ConeMessage.Parse("Line 1\nLine 2"));
			var lines = result.ToString().Split('\n');
			Check.That(() => lines.Length == 3);
			Check.That(
				() => lines[0] == "→ Line 1",
				() => lines[1] == "  Line 2");
		}
	}
}
