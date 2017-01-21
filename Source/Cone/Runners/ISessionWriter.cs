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
		public static void Error(this ISessionWriter writer, string message) {
			writer.Info("→ ");
			var lines = message.Split('\n');
			writer.Important(lines[0]);
			for(var i = 1; i != lines.Length; ++i) {
				writer.Write("\n  ");
				writer.Important(lines[i]);
			}
			writer.Write("\n");
		}
	}
}
