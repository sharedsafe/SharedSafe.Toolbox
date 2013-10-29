using Microsoft.VisualBasic.FileIO;
using VBIO = Microsoft.VisualBasic.FileIO;

namespace Toolbox.IO
{
	public static class RecycleBin
	{
		public static void deleteFile(string path)
		{
			FileSystem.DeleteFile(
				path, 
				UIOption.OnlyErrorDialogs,
				RecycleOption.SendToRecycleBin,
				UICancelOption.ThrowException);
		}
	}
}
