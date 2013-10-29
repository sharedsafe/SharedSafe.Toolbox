using System.Collections.Generic;

namespace Toolbox.Persistence
{
	sealed class ObjectMap_ : ObjectMap
	{
		readonly Dictionary<object, ObjectId> _map = new Dictionary<object, ObjectId>();

		#region ObjectMap Members

		public ObjectId resolveId(object o)
		{
			ObjectId id;
			if (!_map.TryGetValue(o, out id))
				_map[o] = id = new ObjectId((uint)_map.Count);

			return id;
		}

		#endregion
	}
}
