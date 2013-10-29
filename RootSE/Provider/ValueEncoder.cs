using System;
using System.Globalization;
using Newtonsoft.Json;
using Toolbox;

namespace RootSE.Provider
{
	static class ValueEncoder
	{
		public static string encode(object value, IValueSerializer serializer_)
		{
			if (value == null)
				return "NULL";

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
					return escapeString((string)value);

				case TypeCode.Int32:
					return toNumericValue<int>(value);
				case TypeCode.UInt32:
					return toNumericValue<uint>(value);
				case TypeCode.Int64:
					return toNumericValue<Int64>(value);
				case TypeCode.UInt64:
					return toNumericValue<UInt64>(value);
				case TypeCode.Single:
					return ((float)value).ToString(CultureInfo.InvariantCulture.NumberFormat);
				case TypeCode.Double:
					return ((double)value).ToString(CultureInfo.InvariantCulture.NumberFormat);
			}

			if (value is Guid)
				return escapeString(encodeGuid((Guid)value));

			if (serializer_ == null)
				throw new Exception("No Serializer for type {0}".format(value.GetType()));

			return escapeString(serializer_.serialize(value));
		}

		static string encodeGuid(Guid guid)
		{
			return guid.ToString("N");
		}

		public static Guid decodeGuid(string str)
		{
			return new Guid(str);
		}

		public static string escapeString(string value)
		{
			return "'" + value.Replace("'", "''") + "'";
		}

		// we need this conversion to properly convert enum values!

		static string toNumericValue<TypeT>(object value)
		{
			return ((TypeT) value).ToString();
		}
	}
}
