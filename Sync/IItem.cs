using System;
using System.Collections.Generic;
using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	public interface IItem
	{
		/// Every item has a name. This is not normalized, meaning that
		/// if the casing of a letter is changed, the file is treated as a 
		/// different file and the change detection detects a delete and then
		/// a create (which is then probably resolved to a move in a later process)

		string Name { get; }

		/// The type of the item.
		ItemType Type { get; }

		/// Custom attributes (i.e. File Flags, Dates, ACLs, Hashes)
		Attributes Attributes { get; set;  }

		/// Nested items (always sorted and names normalized!)
		/// By default, this synchronizer supports synchronization of hierarchies!

		/// See implementation why this is not an IList
		List<IItem> Nested { get; }
	}

	[Flags]
	public enum ItemCopyFlags
	{
		NoData = 0x01
	}

	public static class IItemExtensions
	{
		public static IItem copyShallow(this IItem prototype)
		{
			return new Item(prototype.Name, prototype.Type, prototype.Attributes);
		}

		public static void addNested(this IItem parent, IItem nested)
		{
			parent.Nested.Add(nested);
		}
	}
}
