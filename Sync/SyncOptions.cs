/**
	Synchronization options.
**/

using System;
using System.Linq;
using System.Collections.Generic;

namespace Toolbox.Sync
{
	public sealed class SyncOptions
	{
		/**
			Use and maintain the folder's last access time to optimize scans, detect differences.
		**/
	
		/// Default false: Don't use it.
		public bool UseFolderLastModificationTime { get; set; }
		/// Compare file/folder flags, default false (flags considered a local property)
		public bool CompareFlags { get; set; }

		/// PreservedFlagMask: Flags that are not significant, but tried to preserved.
		/// Note: if CompareFlags is true, all Flags are implicitly preserved!

		public System.IO.FileAttributes PreservedFlagMask;

		public IItemType[] Types 
		{
			get { return _types ?? SyncFactory.StandardTypes; }
			set { _types = value; }
		}

		IItemType[] _types;

		public override bool Equals(object r)
		{
			return r is SyncOptions && this == (SyncOptions)r;
		}

		public override int GetHashCode()
		{
			return 
				UseFolderLastModificationTime.GetHashCode() ^ 
				CompareFlags.GetHashCode() ^
				PreservedFlagMask.GetHashCode();
		}

		public static bool operator == (SyncOptions l, SyncOptions r)
		{
			return
				l.UseFolderLastModificationTime == r.UseFolderLastModificationTime &&
				l.CompareFlags == r.CompareFlags &&
				l.PreservedFlagMask == r.PreservedFlagMask &&

				l.Types.equalTypes(r.Types);
		}

		public static bool operator !=(SyncOptions l, SyncOptions r)
		{
			return !(l == r);
		}

		// todo: Toolbox Linq Candidate

		public static void requireAllSame(IEnumerable<SyncOptions> options)
		{
			var first = options.First();

			if (!options.All(o => first == o))
				throw new Exception("SyncOptions are required to be the same");
		}

		/// Shallow equality of items (this should not belong here :()

		public bool areItemsEqual(IItem a, IItem b)
		{
			return a.Type == b.Type && Types[a.Type.index()].equals(a, b, this);
		}
	}
}
