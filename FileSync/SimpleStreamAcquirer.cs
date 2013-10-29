using System.IO;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	static class SimpleStreamAcquirer
	{
		public static IAcquiredStream acquireStream(AcquireStreamParam param)
		{
			var stream = new FileStream(param.Path, 
				FileMode.Open, 
				FileAccess.Read, 
				FileShare.ReadWrite | FileShare.Delete);

			return new NativeFileStream(stream);
		}
	}
}
