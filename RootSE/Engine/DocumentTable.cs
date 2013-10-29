using System.Collections.Generic;

namespace RootSE.Engine
{
	sealed class DocumentTable
	{
		public readonly string Name;
		readonly Dictionary<string, ColumnIndex> _indices = new Dictionary<string, ColumnIndex>();

		public DocumentTable(string name)
		{
			Name = name;
		}

		public ColumnIndex tryGetColumnIndex(string name)
		{
			ColumnIndex columnIndex;
			_indices.TryGetValue(name, out columnIndex);
			return columnIndex;
		}

		public void addColumnIndex(ColumnIndex columnIndex)
		{
			_indices.Add(columnIndex.ColumnName, columnIndex);
		}

		public void notifyDocumentsChanged()
		{
			foreach (var index in _indices.Values)
				index.IsSane = false;
		}
	}
}
