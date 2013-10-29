using System.Collections.Generic;
using Toolbox;

namespace RootSE.Provider.RTree
{
	sealed class RTreeIndex : IRTreeIndex
	{
		readonly IRTreeProvider _provider;
		readonly string _name;

		public RTreeIndex(IRTreeProvider provider, string name)
		{
			_provider = provider;
			_name = name;
		}

		public bool indexExists()
		{
			return _provider.hasIndex(_name);
		}

		public void createIndex(uint dimensions)
		{
			_provider.createIndex(_name, dimensions);
		}

		public void deleteIndex()
		{
			_provider.deleteIndex(_name);
		}

		public void insert(ulong id, params Range<float>[] ranges)
		{
			_provider.insert(_name, id, ranges);
		}

		public void update(ulong id, params Range<float>[] ranges)
		{
			_provider.update(_name, id, ranges);
		}

		public void delete(ulong id)
		{
			_provider.delete(_name, id);
		}

		public IEnumerable<ulong> queryOverlapping(params Range<float>[] ranges)
		{
			return _provider.queryOverlapped(_name, ranges);
		}

		public IEnumerable<ORMT> queryOverlapping<ORMT>(string documentTable, string documentIndexColumn, params Range<float>[] ranges)
		{
			return _provider.queryOverlapped<ORMT>(_name, documentTable, documentIndexColumn, ranges);
		}
	}
}
