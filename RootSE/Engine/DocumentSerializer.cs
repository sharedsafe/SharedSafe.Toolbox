using System;
using Newtonsoft.Json;

namespace RootSE.Engine
{
	sealed class DocumentSerializer
	{
		public string serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public object deserialize(string v, Type t)
		{
			return JsonConvert.DeserializeObject(v, t);
		}
	}

	static class DocumentSerializerExtensions
	{
		public static TypeT deserialize<TypeT>(this DocumentSerializer serializer, string v)
		{
			return (TypeT)serializer.deserialize(v, typeof (TypeT));
		}
	}
}
