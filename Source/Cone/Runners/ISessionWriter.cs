using Cone.Core;

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
				else if(e.Style == "info")
					writer.Info(e.ToString());
				else writer.Important(e.ToString());
			}
			writer.Write("\n");
		}
	}
}
