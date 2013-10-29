using System;
using System.Collections.Generic;

namespace RootSE.Provider.SQLite
{
	sealed class FieldReader
	{
		public static Func<ColumnQueryParam, object> resolve(Type type)
		{
			var reader = Reader_ ?? (Reader_ = new FieldReader());
			return reader.internalResolve(type);
		}

		[ThreadStatic] static FieldReader Reader_;

		readonly Dictionary<Type, Func<ColumnQueryParam, object>> _retrieveTypedColumnValue = new Dictionary<Type, Func<ColumnQueryParam, object>>();

		Func<ColumnQueryParam, object> internalResolve(Type type)
		{
			Func<ColumnQueryParam, object> ff;
			if (!_retrieveTypedColumnValue.TryGetValue(type, out ff))
			{
				Type td = typeof(TypeDependentMethods<>).MakeGenericType(type);
				var f = td.GetField("sqlite3_column_object");
				var fv = f.GetValue(null);
				ff = (Func<ColumnQueryParam, object>)fv;
				_retrieveTypedColumnValue.Add(type, ff);
			}

			return ff;
		}
	}
}
