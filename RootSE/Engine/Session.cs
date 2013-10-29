using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox;

namespace RootSE.Engine
{
	/**
		A session stores all objects returned from the engine and maps them to ids.
	**/

	sealed class Session : ISession
	{
		Dictionary<object, long> _ids_;

		public IDisposable begin()
		{
			if (_ids_ != null)
				throw new Exception("Session is already active.");

			_ids_ = new Dictionary<object, long>();


			return new DisposeAction(() =>
				{
					Debug.Assert(_ids_ != null);
					_ids_ = null;
				});
		}

		public long idOf(object instance)
		{
			if (!IsActive)
				throw new Exception("Session not active, failed to retrieve Id for object.");


			long r;
			if (!_ids_.TryGetValue(instance, out r))
				throw new Exception("Failed to retrieve id for instance {0}".format(instance));

			return r;
		}

		public void register(object instance, long id)
		{
			if (!IsActive)
				return;

			_ids_.Add(instance, id);
		}

		public void unregister(object obj)
		{
			if (!IsActive)
				return;

			if (!_ids_.Remove(obj))
				throw new Exception("Failed to remove object {0} from id table.".format(obj));
		}

		bool IsActive
		{
			get { return _ids_ != null;}
		}
	}
}
