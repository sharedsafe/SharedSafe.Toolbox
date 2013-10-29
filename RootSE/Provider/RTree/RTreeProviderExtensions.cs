namespace RootSE.Provider.RTree
{
	public static class RTreeProviderExtensions
	{
		public static IRTreeIndex index(this IRTreeProvider provider, string indexName)
		{
			return new RTreeIndex(provider, indexName);
		}
	}
}
