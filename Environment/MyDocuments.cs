using System.IO;

namespace Toolbox.Environment
{
	public static class MyDocuments
	{
		public static string makePath(string filenameOrDirectory)
		{
			return Path.Combine(ApplicationDirectory, filenameOrDirectory);
		}

		public static readonly string ApplicationDirectory = makeDocumentsDirectory();

		static string makeDocumentsDirectory()
		{
			var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var product = System.Windows.Forms.Application.ProductName;
			var directory = Path.Combine(documentsPath, product);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			return directory;
		}
	}
}
