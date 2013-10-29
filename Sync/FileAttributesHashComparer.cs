namespace Toolbox.Sync
{
	public static class FileAttributesHashComparer
	{
		public static bool equalBasedOnHash(FileAttributes l, FileAttributes r)
		{
			if (l.Length != r.Length)
				return false;

			if (l.Hash == null || r.Hash == null)
				return false;

			return equalBasedOnHash(l.Hash, r.Hash);
		}

		// this is intentially not in HashBlocks, comparison of hashes does not make sense without
		// comparing the length.

		public static bool equalBasedOnHash(HashBlocks l, HashBlocks r)
		{
			return 
				l.Format == r.Format &&
					l.BlockSize == r.BlockSize &&
						equalBlocks(l.Blocks, r.Blocks);
		}

		static bool equalBlocks(string[] l, string[] r)
		{
			if (l.Length != r.Length)
				return false;

			for (int i = 0; i != l.Length; ++i)
				if (l[i] != r[i])
					return false;

			return true;
		}
	}
}