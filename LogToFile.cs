using System;
using System.IO;

namespace Toolbox
{
	public static class LogToFile
	{
		public static IDisposable append(string filename)
		{
			var f = File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
			var sw = new StreamWriter(f);

			Action<string, string[]> writeLine = LogToFile.writeLine(sw, f);

			var context = Log.pushOutputContext(writeLine);

			return new DisposeAction(() =>
				{
					context.Dispose();
					sw.Dispose();
					f.Close();
				});
		}

		public static void appendGlobally(string filename)
		{
			var f = File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
			var sw = new StreamWriter(f);
			Log.GlobalWriteLines = writeLine(sw, f); 
		}

		static Action<string, string[]> writeLine(TextWriter sw, Stream f)
		{
			return (prefix, lines) =>
			{
				if (prefix != string.Empty)
					prefix += ": ";

				foreach (var line in lines)
				{
					sw.WriteLine(prefix + line);
				}

				sw.Flush();
				f.Flush();
			};
		}
	}
}
