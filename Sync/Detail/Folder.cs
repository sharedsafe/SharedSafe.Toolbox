using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	sealed class Folder : IItemType
	{
		#region IItemType Members

		const ItemType ThisType = ItemType.Folder;

		public uint Index
		{
			get { return (uint)ThisType; }
		}

		public void diff(IScope scope, IItem a, IItem b, IDiffCollector diffs)
		{
			Debug.Assert(a.Name == b.Name);

			DiffAlgorithm.compareNested(scope, a, b, diffs);

			if (equals(a, b, diffs.Options))
				return;

			diffs.collect(scope, a.Name, DiffKind.Changed);
		}

		public bool equals(IItem source, IItem target, SyncOptions options)
		{
			Debug.Assert(source.Name == target.Name);
			Debug.Assert(source.Name != string.Empty);
			Debug.Assert(source.Type == ThisType);
			Debug.Assert(target.Type == ThisType);

			var sa = source.Attributes as FolderAttributes;
			var ta = target.Attributes as FolderAttributes;
			Debug.Assert(sa != null && ta != null);

			return 
				(!options.CompareFlags || sa.Flags == ta.Flags) && 
				(!options.UseFolderLastModificationTime || sa.LastModificationTime == ta.LastModificationTime);
		}

		#endregion

		public static readonly IItemType Type = new Folder();

		#region IItemType Members

		public void copySafe(IItemContext from, IItemContext to, IRelocationContext context)
		{
			// after is null, so we create the directory (having the same attributes?)

			// todo: separate directory creation and modification of attributes clearly, 
			// this function looks like a mess.

			var toFS = to.Replica.FileSystem;

			// target is not existing, do create it!
			if (to.Item_ == null)
			{
				toFS.createFolder(from, to, context);
				return;
			}

			// copy directory attributes (but only if it is required)
			if (!equals(from.Item_, to.Item_, context.Options))
				toFS.setFolderAttributes(to, (FolderAttributes)from.Item_.Attributes, context);
		}

		#endregion

		#region IItemType Members

		public IItem merge(ItemContext source, ItemContext target, IRelocationContext context)
		{
			var options = context.Options;

			Debug.Assert(source.Item_.Name == target.Item_.Name);
			Debug.Assert(source.Item_.Type == target.Item_.Type);

			var sa = (FolderAttributes)source.Item_.Attributes;
			var ta = (FolderAttributes)target.Item_.Attributes;

			var newAttributes = new FolderAttributes
			{
				Flags = (sa.Flags | ta.Flags),
				LastModificationTime = (sa.LastModificationTime > ta.LastModificationTime
					? sa.LastModificationTime
					: ta.LastModificationTime)
			};

			if (
				(options.CompareFlags && sa.Flags != newAttributes.Flags) ||
				(options.UseFolderLastModificationTime && sa.LastModificationTime != newAttributes.LastModificationTime))
			{
				// hmm: source file system should never be modified in some situations I guess :(

				var sourceFS = source.Replica.FileSystem;
				sourceFS.setFolderAttributes(source, newAttributes, context);
			}

			if ((options.CompareFlags && ta.Flags != newAttributes.Flags) ||
				(options.UseFolderLastModificationTime && ta.LastModificationTime != newAttributes.LastModificationTime))
			{
				var targetFS = target.Replica.FileSystem;
				targetFS.setFolderAttributes(target, newAttributes, context);
			}

			// note: after the merge, no file system item is attached
			return new Item(source.Item_.Name, ItemType.Folder, newAttributes);
		}
		#endregion

		
	}
}
