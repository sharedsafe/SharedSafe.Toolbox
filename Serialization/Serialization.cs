using System.IO;

namespace Toolbox.Serialization
{
	public static class Serialization
	{
		#region Serialization

		// todo: implement some kind of TypeTraits, so that we don't 
		// need to instantiate the context and the serializer all the time.
		// todo: check the JSonEx serializer, because of its recursive nature, the type 
		// dependency on the serializer may not actually exist!

		public static string serialize<T>(this T obj)
			where T : class
		{
			if (obj == null)
				throw new InternalError("Tried to serialize null");

			return typeof(T).serializer().Serialize(obj);
		}

		public static void serializeTo<T>(this T obj, Stream stream)
			where T : class
		{
			if (obj == null)
				throw new InternalError("Tried to serialize null");

			typeof(T).serializer().Serialize(obj, stream);
		}

		public static void serializeTo<T>(this T obj, TextWriter tw)
			where T : class
		{
			if (obj == null)
				throw new InternalError("Tried to serialize null");

			typeof(T).serializer().Serialize(obj, tw);
		}

		#endregion

		#region Deserialization

		// note: if an interface is given, we need to resolve a 
		// TypeSpace attribute of G4 to deserialize properly!

		public static T deserialize<T>(this string str)
			where T : class
		{
			var r = (T)typeof(T).serializer().Deserialize(str);
			if (r == null)
				throw new InternalError("Deserialized null");

			return r;
		}

		public static T deserialize<T>(this Stream stream)
			where T : class
		{
			var r = (T)typeof(T).serializer().Deserialize(stream);
			if (r == null)
				throw new InternalError("Deserialized null");

			return r;
		}

		public static T deserialize<T>(this TextReader textReader)
			where T : class
		{
			var r = (T)typeof(T).serializer().Deserialize(textReader);
			if (r == null)
				throw new InternalError("Deserialized null");

			return r;
		}

		#endregion

		public static T cloneUsingSerialize<T>(this T obj)
			where T : class
		{
			return obj.serialize().deserialize<T>();
		}
	}
}
