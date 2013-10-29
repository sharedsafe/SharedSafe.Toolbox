using System;
using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	/**
		Root folder is treated a little different, additionally it does not expect a scope.
	**/
	
	sealed class RootFolder : IItemType
	{
		const ItemType ThisType = ItemType.RootFolder;

		public static IItem createItem()
		{
			return SyncFactory.createItem(string.Empty, ItemType.RootFolder, null);
		}



		public static readonly IItemType Type = new RootFolder();
		
		#region IItemType Members

		#region IItemType Members

		public uint Index
		{
			get { return (uint)ThisType; }
		}

		#endregion


		public void diff(IScope scope, IItem a, IItem b, IDiffCollector diffs)
		{
			Debug.Assert(scope == null && a.Name == b.Name && a.Name == string.Empty);
			DiffAlgorithm.compareNested(null, a, b, diffs);
		}

		public bool equals(IItem source, IItem target, SyncOptions options)
		{
			Debug.Assert(source.Name == string.Empty && target.Name == string.Empty);
			Debug.Assert(source.Type == ThisType);
			Debug.Assert(target.Type == ThisType);
			return true;
		}

		public void copySafe(IItemContext source, IItemContext target, IRelocationContext context)
		{
			throw new NotImplementedException("Can not copy root folders");
		}

		public IItem merge(ItemContext source, ItemContext target, IRelocationContext context)
		{
			// merging of root folders is simply creating a new one with a name, attributes are not supported 
			// at a root folder.

			Debug.Assert(source.Name == target.Name);
			Debug.Assert(source.Name == string.Empty);

			return createItem();
		}

		#endregion

	}
}
