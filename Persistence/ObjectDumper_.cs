using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox.Persistence
{
	sealed class ObjectDumper_  : ObjectWriter
	{
		#region ObjectWriter Members

		public IDisposable writeInstance(MetaType type, object instance, ObjectId id)
		{
			this.D("Beginning writing instance: " + type);


			return new DisposeAction(() =>
				{
					this.D("Ending writing instance: " + type);
				});
		}

		public IDisposable writeNamed(MetaField field)
		{
			this.D("Beginning writing field: " + field);

			return new DisposeAction(() =>
				{
					this.D("Ending writing field: " + field);
				});
		}

		public void writeNamedValue(string name, object value)
		{

		}

		#endregion
	}
}
