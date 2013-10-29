using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox.Persistence
{
	interface ObjectMap
	{
		// Resolves a unique id for an object.
		ObjectId resolveId(object o);
	}
}
