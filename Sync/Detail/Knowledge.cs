using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	sealed class Knowledge : IKnowledge
	{
		public Knowledge(IItem root)
		{
			Debug.Assert(root.Type == ItemType.RootFolder);
			RootItem = root;
		}

		#region IKnowledge Members

		public IItem RootItem { get; private set; }
		
		#endregion
	}
}
