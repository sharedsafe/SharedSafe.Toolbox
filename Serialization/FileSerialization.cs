using System;
using System.IO;

namespace Toolbox.Serialization
{
	public static class FileSerialization
	{
		#region Serialization (Toolbox candidate)
	
		public static void serializeToFile<ObjT>(this ObjT obj, string fn)
			where ObjT : class
		{
			// pre-check null, otherwise we would create an empty file and
			// deserialization would break, too
			if (obj == null)
				throw new InternalError("Tried to serialize null");

			using (var stream = new StreamWriter(fn))
				obj.serializeTo(stream);
		}
	
		public static ObjT deserializeFromFile<ObjT>(this string fn)
			where ObjT: class
		{
			return deserializeFromFile<ObjT>(fn, () =>
				{
					throw new Exception("File does not exist: " + fn);
				});
		}

		public static ObjT deserializeFromFile<ObjT>(this string fn, Func<ObjT> def)
			where ObjT : class
		{
			if (!File.Exists(fn))
				return def();

			using (var stream = File.OpenRead(fn))
			{
				return stream.deserialize<ObjT>();
			}
		}

		#endregion
	}
}