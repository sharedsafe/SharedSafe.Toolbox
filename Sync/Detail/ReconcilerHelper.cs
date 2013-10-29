using System.Collections.Generic;
using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	static class ReconcilerHelper
	{

		/// Note: both, folder A and B may be null

		public static Dictionary<string, Two<IItem>> combineNestedItems(IItem folderA, IItem folderB)
		{
			var dict = new Dictionary<string, Two<IItem>>();

			if (folderA != null)
			{
				foreach (var na in folderA.Nested)
				{
					// we assume there are no duplicates, so we can simply add the first one.
					dict.Add(na.Name, Two.make(na, null));
				}
			}

			if (folderB == null)
				return dict;

			foreach (var nb in folderB.Nested)
			{
				Two<IItem> items;

				if (dict.TryGetValue(nb.Name, out items))
				{
					Debug.Assert(items.Second == null);
					// note: Pair is a value type, so we need to overwrite it in the dictionary!
					dict[nb.Name] = Two.make(items.First, nb);
				}
				else
					dict.Add(nb.Name, Two.make(null, nb));
			}

			return dict;
		}

		public static IDirtyPath resolveNestedDirty(IDirtyPath folder, string name)
		{
			Debug.Assert(folder != null);
			IDirtyPath r;
			if (!folder.Nested.TryGetValue(name, out r))
				return null;

			return r ?? DirtyLeafPath;
		}

		static readonly IDirtyPath DirtyLeafPath = new DirtyPath(null);

		/**
			A number of different states must be handled in reconcillation.
		
			general assertion:
		
			one of the items must be set, one of the dirties must be set.

			So we have the following combinations:
		            1  2  4  8
					D0 D1 I0 I1		i
			0:		*     *			5
			1:		   *  *			6
			2:		*  *  *			7
			3:		*        *		9
			4:		   *     *		10
			5:		*  *     *		11
			6:		*     *  *		13
			7:		   *  *  *		14
			8:		*  *  *  *		15
					
			update/delete conflicts: direct conflicts (can not handle without user interaction, won't land here!)

			2:		conflict: I0 updated, I1 deleted
			5:		conflict: I1 updated, I0 deleted
		
			update/update conflict: content conflict (may be able to be handled dependening on type, dirs can be handled)
		
			8:		(may be) conflict, both updated .. directories may resolve.

			new: target is not existing, must copy
	
			0:
			4:

			delete: target must be deleted

			1:
			3:

			copy: target existing and must be copied over!

			6:
			7:
		
		*/

		public static uint classify(IItem[] items, IDirtyPath[] dirty)
		{
			// one of the two items must be set
			Debug.Assert(items[0] != null || items[1] != null);
			// one of the two must be dirty
			Debug.Assert(dirty[0] != null || dirty[1] != null);

			int c = 0;
			c += dirty[0] != null ? 1 : 0;
			c += dirty[1] != null ? 2 : 0;
			c += items[0] != null ? 4 : 0;
			c += items[1] != null ? 8 : 0;

			return States[c].Value;
		}

		static readonly uint?[] States = new uint?[] 
		{
			null, null, null, null, null,
			0, 1, 2, null,
			3, 4, 5, null,
			6, 7, 8
		};

	}
}
