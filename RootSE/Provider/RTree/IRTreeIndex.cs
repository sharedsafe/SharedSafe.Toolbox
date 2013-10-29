using System.Collections.Generic;
using RootSE.ORM;
using Toolbox;

namespace RootSE.Provider.RTree
{
	public interface IRTreeIndex
	{
		#region Schema

		bool indexExists();
		void createIndex(uint dimensions);
		void deleteIndex();

		#endregion

		void insert(ulong id, params Range<float>[] ranges);
		void update(ulong id, params Range<float>[] ranges);
		void delete(ulong id);

		IEnumerable<ulong> queryOverlapping(params Range<float>[] ranges);

		// note: in a range query, begin is included, end is not!
		// the orm type is created by the document table and must contain a primary index.

		IEnumerable<ORMT> queryOverlapping<ORMT>(string documentTable, string documentIdColumn, params Range<float>[] ranges);
	}

	public static class RTreeIndexExtensions
	{
		public static IEnumerable<TypeT> queryOverlapped<TypeT>(this IRTreeIndex index, Repository<TypeT> t, params Range<float>[] ranges) 
			where TypeT : class
		{
			return index.queryOverlapping<TypeT>(t.TableName, t.PrimaryKeyColumn, ranges);
		}
	}
}
