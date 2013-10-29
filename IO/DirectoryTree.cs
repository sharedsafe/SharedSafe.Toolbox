using System.IO;

namespace Toolbox.IO
{
	public static class DirectoryTree
	{
		public static void copy(string source, string target)
		{
			copy(new DirectoryInfo(source), new DirectoryInfo(target) );
		}

		static void copy(DirectoryInfo source, DirectoryInfo target)
		{
			if (!Directory.Exists(target.FullName))
				Directory.CreateDirectory(target.FullName);

			// Copy each file into it’s new directory.
			foreach (var fi in source.GetFiles())
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

			// Copy each subdirectory using recursion.
			foreach (var diSourceSubDir in source.GetDirectories())
			{
				var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				copy(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}
