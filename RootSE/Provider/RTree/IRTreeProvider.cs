using System.Collections.Generic;
using Toolbox;
using Toolbox.Constraints;

namespace RootSE.Provider.RTree
{
	[Pure]
	public interface IRTreeProvider
	{
		#region Schema
	
		bool hasIndex(string indexName);
		void createIndex(string indexName, uint dimensions);
		void deleteIndex(string indexName);

		#endregion

		void insert(string indexName, ulong id, params Range<float>[] ranges);
		void update(string indexName, ulong id, params Range<float>[] ranges);
		void delete(string indexName, ulong id);

		IEnumerable<ulong> queryOverlapped(string indexName, params Range<float>[] ranges);

		// note: in a range query, begin is included, end is not!
		// the orm type is created by the document table and must contain a primary index.

		IEnumerable<ORMT> queryOverlapped<ORMT>(string indexName, string documentTable, string documentIndexColumn, params Range<float>[] ranges);
	}
}
