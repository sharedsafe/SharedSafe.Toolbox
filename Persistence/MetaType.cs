using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox.Persistence
{
	sealed class MetaType
	{
		readonly Type _type;

		public MetaType(Type t)
		{
			_type = t;
		}
	}
}
