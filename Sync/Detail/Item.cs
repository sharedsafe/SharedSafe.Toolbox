using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Toolbox.Serialization;

namespace Toolbox.Sync.Detail
{
	/**
		Default implementation for Item

		This is public because of serialization purposes!
	**/

	[Obfuscation]
	[TypeAlias("Sync.Item")]
	sealed class Item : IItem
	{
		List<IItem> _nested;

		public Item(string name, ItemType type, Attributes attributes)
		{
			Debug.Assert(name != null);

			Name = name;
			Type = type;
			Attributes = attributes;
		}

		#region IItem Members

		public string Name { get; private set; }

		public ItemType Type { get; private set; }

		public Attributes Attributes { get; set; }

		/// Note: we use the explicit List type, because otherwise our Serializer 
		/// would fully qualify the type, which we don't want.

		public List<IItem> Nested
		{
			get { return _nested ?? (_nested = new List<IItem>()); }
			// required for serialization 
			// (though serialization should go with the setter only in case of IList)
			set { _nested = value; }
		}

		// This is here for legacy serialization purposes only (Data is not used anymore)
		public object Data
		{
			set { }
			get { return null; }
		}

		#endregion

		public override string ToString()
		{
			return Name + "(" + Type + ")";
		}
	}
}
