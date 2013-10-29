using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	public static class SyncFactory
	{
		public static ISession createSession(params IReplica[] replicas)
		{
			return createSession(createKnowledge(), replicas);
		}

		public static ISession createSession(IKnowledge knowledge, params IReplica[] replicas)
		{
			return new Session(knowledge, replicas);
		}

		public static IKnowledge createKnowledge()
		{
			return createKnowledge(createRootItem());
		}

		public static IKnowledge createKnowledge(IItem rootItem)
		{
			return new Knowledge(rootItem);
		}

		public static IScope getRootScope()
		{
			return Scope.Root;
		}

		#region Item Creation

		public static IItem createRootItem()
		{
			return RootFolder.createItem();
		}

		public static IItem createFileItem(string name, FileAttributes attributes)
		{
			return createItem(name, ItemType.File, attributes);
		}

		public static IItem createFolderItem(string name, FolderAttributes attributes)
		{
			return createItem(name, ItemType.Folder, attributes);
		}

		public static IItem createItem(string name, ItemType type, Attributes attributes)
		{
			return new Item(name, type, attributes);
		}

		#endregion

		#region Standard IItemTypes

		public static readonly IItemType[] StandardTypes = new[] 
		{
			File.Type,
			Folder.Type,
			RootFolder.Type
		};

		#endregion

		public static bool equalTypes(this IItemType[] l, IItemType[] r)
		{
			if (l == r)
				return true;

			if (l == null || r == null)
				return false;

			if (l.Length != r.Length)
				return false;

			foreach (var i in l.indices())
				if (l[i] != r[i])
					return false;

			return true;
		}
	}
}
