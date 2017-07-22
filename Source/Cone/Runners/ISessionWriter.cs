using Cone.Expectations;

namespace Cone.Runners
{
	public interface ISessionWriter
	{
		void Write(string format, params object[] args);
		void Important(string format, params object[] args);
		void Info(string format, params object[] args);
	}

	public static class SessionWriterExtensions
	{
		public static void Error(this ISessionWriter writer, ConeMessage message) {
			writer.Info("→ ");
			foreach(var e in message) {
				if(e == ConeMessageElement.NewLine)
					writer.Write("\n  ");
				else switch(e.Style) {
					default: writer.Important(e.ToString()); break;
					case "info": writer.Info(e.ToString()); break;
					case "expression": writer.Write(e.ToString()); break;
				}
			}
			writer.Write("\n");
		}
	}
}
