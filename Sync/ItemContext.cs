using System.Diagnostics;

namespace Toolbox.Sync
{
	public sealed class ItemContext : IItemContext
	{
		/// Test if we could reduce Replica dependency to IFS dependency!
		/// => no: if we have no Item, we can't decide for th eproper FS :(
		/// We could however pass a Func<uint>:IFS here to remote the IReplica dependency!

		public ItemContext(string name, IScope scope, IItem item_, IReplica replica)
		{
			/// note: that IItem may be null for copy on targets!
			Debug.Assert(name != null && replica != null);

			Name = name;
			Replica = replica;
			Scope_ = scope;
			Item_ = item_;
		}

		public string Name { get; private set; }
		public IItem Item_ { get; private set; }
		public IReplica Replica { get; private set; }
		public IScope Scope_ { get; private set; }
	}

	public interface IItemContext
	{
		string Name { get; }
		IReplica Replica { get; }
		IItem Item_ { get; }
		IScope Scope_ { get; }
	}

	public static class ItemContextExtensions
	{
		public static IAcquiredStream acquireFileStream(this IItemContext item, IRelocationContext context)
		{
			return item.Replica.FileSystem.acquireFileStream(item, context);
		}

		public static bool isFolder(this IItemContext item)
		{
			Debug.Assert(item.Item_ != null);
			return item.Item_.Type != ItemType.File;
		}

		public static string makeAbsolutePath(this IItemContext item)
		{
			// note: in case of root folders, scope could be null :(
			return item.Replica.makeAbsolutePath(item.Scope_ != null ? combine(item.Scope_.makePath(), item.Name) : item.Name);
		}

		// returns an empty string for the root directory.

		public static string getRelativeDirectory(this IItemContext item)
		{
			return item.Scope_ != null ? item.Scope_.makePath() : string.Empty;
		}
			
		static string combine(string l, string r)
		{
			if (l == "")
				return r;
			return l + "/" + r;
		}
	}
}