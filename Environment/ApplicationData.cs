using System.Diagnostics;
using System.IO;
using Env = System.Environment;


namespace Toolbox.Environment
{
	public static class ApplicationData
	{
		public static string makeLocalPath(string filenameOrDirectory)
		{
			return Path.Combine(LocalApplicationDirectory, filenameOrDirectory);
		}

		static readonly string LocalApplicationDirectory = makeLocalApplicationDirectory();

		static string makeLocalApplicationDirectory()
		{
			var dataPath = Env.GetFolderPath(Env.SpecialFolder.LocalApplicationData);
			var product = System.Windows.Forms.Application.ProductName;
			var directory = Path.Combine(dataPath, product);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			Debug.Assert(Directory.Exists(directory));
			return directory;
		}
	}
}
