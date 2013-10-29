using System.IO;

namespace Toolbox.IO
{
	/**
		File operations that _try_ to be transactional.
	**/

	public static class AtomicFileOperations
	{
		public static void overwriteTextFile(string path, string content)
		{
			var tmpPath = path + ".tmp";
			File.WriteAllText(tmpPath, content);
			if (File.Exists(path))
				File.Replace(tmpPath, path, null);
			else
				File.Move(tmpPath, path);
		}
	}
}
