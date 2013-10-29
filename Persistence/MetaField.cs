using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Toolbox.Persistence
{
	sealed class MetaField
	{
		readonly FieldInfo _info;

		public MetaField(FieldInfo info)
		{
			_info = info;
		}
	}
}
