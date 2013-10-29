using System;

namespace RootSE.Provider
{
	public interface IValueSerializer
	{
		string serialize(object instance);
		object deserialize(string serialized, Type type);
	}
}
