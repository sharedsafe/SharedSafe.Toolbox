using System.Collections.Generic;
using System.Linq;
using RootSE.Provider.SQLite;
using Toolbox;
using Toolbox.Constraints;

namespace RootSE.Provider.RTree
{
	[Pure]
	sealed class RTreeProvider : IRTreeProvider
	{
		const string IndexTableSuffix = "_RTree";
		const string MinColumnPrefix = "begin";
		const string MaxColumnPrefix = "end";
		const string IndexIdColumnName = "id";

		readonly SQLiteImplementation _db;
		readonly SQLiteProvider _storageProvider;

		public RTreeProvider(SQLiteImplementation db, SQLiteProvider storageProvider)
		{
			_db = db;
			_storageProvider = storageProvider;
		}

		public bool hasIndex(string indexName)
		{
			return _storageProvider.hasTable(makeIndexTableName(indexName));
		}

		public void createIndex(string indexName, uint dimensions)
		{
			var tableName = makeIndexTableName(indexName);

			var dimensionColumnNames = string.Join(",", makeDimensionColumnNames().Take((int)dimensions*2).ToArray());

			_db.ExecuteNonQuery("CREATE VIRTUAL TABLE {0} USING rtree({1},{2})".
				format(Escape.table(tableName), IndexIdColumnName, dimensionColumnNames));
		}

		public void deleteIndex(string indexName)
		{
			_db.ExecuteNonQuery("DROP TABLE " + Escape.table(makeIndexTableName(indexName)));
		}

		#region Insert / Update / Delete

		public void insert(string indexName, ulong id, params Range<float>[] ranges)
		{
			var tableName = makeIndexTableName(indexName);

			ColumnValue[] values = makeColumnValues(id, ranges);
			_storageProvider.insert(tableName, values);
		}

		public void update(string indexName, ulong id, params Range<float>[] ranges)
		{
			var tableName = makeIndexTableName(indexName);

			ColumnValue[] values = makeColumnValues(id, ranges);
			_storageProvider.update(tableName, values, Term.column(IndexIdColumnName).equals(id));
		}

		public void delete(string indexName, ulong id)
		{
			var tableName = makeIndexTableName(indexName);
			_storageProvider.delete(tableName, new ColumnValue(IndexIdColumnName, id));
		}

		#endregion
		
		#region Query

		public IEnumerable<ulong> queryOverlapped(string indexName, params Range<float>[] ranges)
		{
			var tableName = makeIndexTableName(indexName);

			var queryTerm = createOverlappingQueryCriteria(ranges);

			return _storageProvider.queryColumn<ulong>(tableName, IndexIdColumnName, queryTerm);
		}

		public IEnumerable<ORMT> queryOverlapped<ORMT>(string indexName, string documentTable, string documentPrimaryIndexColumn, params Range<float>[] ranges)
		{
			var indexTableName = makeIndexTableName(indexName);
			var joinCriteria =
				Term.column(documentTable, documentPrimaryIndexColumn).equals(Term.column(indexTableName, IndexIdColumnName));

			var overlappingCriteria = createOverlappingQueryCriteria(ranges);

			var completeCriteria = joinCriteria.And(overlappingCriteria);

			return _storageProvider.query<ORMT>(new[] {documentTable, indexTableName}, completeCriteria.SQL);
		}

		static TermCriteria createOverlappingQueryCriteria(IEnumerable<Range<float>> ranges)
		{
			var rangeQueries = createRangeQueries(ranges);

			return rangeQueries.Aggregate((left, right) => left.And(right));
		}

		static IEnumerable<TermCriteria> createRangeQueries(IEnumerable<Range<float>> ranges)
		{
			using (var names = makeDimensionColumnNamePairs().GetEnumerator())
			{
				foreach (var range in ranges)
				{
					var columnNames = names.next();
					yield return Term.column(columnNames.First).less(range.End);
					yield return Term.column(columnNames.Second).greaterOrEqual(range.Begin);
				}
			}
		}

		#endregion

		#region Helper

		static string makeIndexTableName(string indexName)
		{
			return indexName + IndexTableSuffix;
		}

		static IEnumerable<string> makeDimensionColumnNames()
		{
			for (var dimensionIndex=0;;++dimensionIndex)
			{
				yield return MinColumnPrefix + dimensionIndex;
				yield return MaxColumnPrefix + dimensionIndex;
			}
		}

		static IEnumerable<Two<string>> makeDimensionColumnNamePairs()
		{
			return makeDimensionColumnNames().pair();
		}

		static ColumnValue[] makeColumnValues(ulong id, Range<float>[] ranges)
		{
			var r = new ColumnValue[ranges.Length*2 + 1];

			r[0] = new ColumnValue(IndexIdColumnName, id);

			var it = 1;
			using (var names = makeDimensionColumnNames().GetEnumerator())
			{
				foreach (var range in ranges)
				{
					names.MoveNext();
					var firstName = names.Current;
					names.MoveNext();
					var secondName = names.Current;


					r[it++] = new ColumnValue(firstName, range.Begin);
					r[it++] = new ColumnValue(secondName, range.End);
				}
			}

			return r;
		}


		#endregion
	}
}
