using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	sealed class File : IItemType
	{
		#region IItemType Members

		const ItemType ThisType = ItemType.File;

		public uint Index
		{ get { return (uint)ThisType; } }

		public void diff(IScope scope, IItem a, IItem b, IDiffCollector diffs)
		{
			Debug.Assert(scope != null);

			if (equals(a, b, diffs.Options))
				return;

			diffs.collect(scope, a.Name, DiffKind.Changed);
		}


		public bool equals(IItem source, IItem target, SyncOptions options)
		{
			Debug.Assert(source.Name == target.Name);
			Debug.Assert(source.Type == ThisType);
			Debug.Assert(target.Type == ThisType);

			var sa = (FileAttributes)source.Attributes;
			var ta = (FileAttributes)target.Attributes;


			return
				(!options.CompareFlags || sa.Flags == ta.Flags) &&
				sa.Length == ta.Length &&
				equalHashOrLastModificationTime(sa, ta);
		}

		static bool equalHashOrLastModificationTime(FileAttributes l, FileAttributes r)
		{
			// if we do have hashes on both sides, we _only_ compare hashes

			if (l.Hash != null && r.Hash != null)
				return FileAttributesHashComparer.equalBasedOnHash(l, r);

			// otherwise (legacy) we compare the last modification time.
			// This should be last cross-repository comparison of Last Modification Times (and
			// should never happen, because we always generate hashes for future files)

			return l.LastModificationTime == r.LastModificationTime;
		}

		#endregion

		public static readonly IItemType Type = new File();

		#region IItemType Members

		public void copySafe(IItemContext from, IItemContext to, IRelocationContext context)
		{
			// todo: do the actual copying safe (check target attributes before and so on)
			// todo: check if this is actually transaction safe on an error (for long files for example).

			var toFS = to.Replica.FileSystem;

			// todo: may merge delete and create()

			if (to.Item_ == null)
				toFS.createFile(from, to, context);
			else
				toFS.modifyFile(from, to, context);
		}

		public IItem merge(ItemContext source, ItemContext target, IRelocationContext context)
		{
			// right now, we don't support attribute or file merging!
			return null;
		}

		#endregion
	}
}
