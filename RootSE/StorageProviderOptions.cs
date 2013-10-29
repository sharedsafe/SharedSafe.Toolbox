using RootSE.Provider;

namespace RootSE
{
	public struct StorageProviderOptions
	{
		// Write asynchronous to the database (don't wait for data to sync to the hd). May
		// corrupt the database in case of power outages.
		public bool WriteAsynchronous;
		public IValueSerializer Serializer_;
	}
}
