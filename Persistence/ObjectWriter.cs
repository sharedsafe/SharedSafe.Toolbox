using System;

namespace Toolbox.Persistence
{
	interface ObjectWriter
	{
		IDisposable writeInstance(MetaType type, ObjectId id);
		IDisposable writeField(MetaField field);
		void writeValue(MetaField field, object value);
	}
}
