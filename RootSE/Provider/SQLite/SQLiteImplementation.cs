using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using Toolbox;
using Toolbox.Constraints;

// from: http://www.switchonthecode.com/tutorials/csharp-tutorial-writing-a-dotnet-wrapper-for-sqlite

namespace RootSE.Provider.SQLite
{
	[Pure]
	sealed class SQLiteImplementation : IDisposable
	{
		readonly IValueSerializer _serializer;
		readonly IntPtr _db; //pointer to SQLite database

		/// <summary>
		/// Opens or creates SQLite database with the specified path
		/// </summary>
		/// <param name="path">Path to SQLite database</param>
		public SQLiteImplementation(string path, IValueSerializer serializer)
		{
			if (SQLite.open(path, out _db) != SQLite.OK)
				throw new SQLiteException(
					"Could not open database file: " + path);

			_serializer = serializer;
		}

		/// <summary>
		/// Closes the SQLite database
		/// </summary>
		public void Dispose()
		{
			SQLite.close(_db);
		}

		/// <summary>
		/// Executes a query that returns no results
		/// </summary>
		/// <param name="query">SQL query to execute</param>
		public void ExecuteNonQuery(string query)
		{
			this.T(query);

			//prepare the statement
			IntPtr stmHandle = Prepare(query);
			try
			{
				var stepResult = SQLite.step(stmHandle);

				switch (stepResult)
				{
					case SQLite.ROW:
						this.W("Non-Query returned a row.");
						break;

					case SQLite.DONE:
						break;

					default:
						throw new SQLiteException(
						  "Could not execute SQL statement: " + Marshal.PtrToStringUni(SQLite.errmsg(_db)));
				}
			}
			finally
			{
				Finalize(stmHandle);
			}
		}

		public ulong LastInsertId
		{
			get
			{
				// this is actually unsigned!!!!
				return (ulong)SQLite.lastInsertRowId(_db);
			}
		}

		public uint Changes
		{
			get { return (uint) SQLite.changes(_db); }
		}

		/// <summary>
		/// Executes a query and stores the results in 
		/// a DataTable
		/// </summary>
		/// <param name="query">SQL query to execute</param>
		/// <returns>DataTable of results</returns>
		public DataTable ExecuteQuery(string query)
		{
			this.T(query);
			//prepare the statement
			IntPtr stmHandle = Prepare(query);

			try
			{
				//get the number of returned columns
				int columnCount = SQLite.columnCount(stmHandle);

				//create datatable and columns
				var dTable = new DataTable();
				for (int i = 0; i < columnCount; i++)
				{
					// we don't require column names for our internal queries! So don't require
					// sqlite to provide METADATA (which is not available on Mac, for example)

					// var columnName = Marshal.PtrToStringUni(SQLite.columnOriginName(stmHandle, i));
					var columnName = string.Empty;
					dTable.Columns.Add(columnName);
				}

				//populate datatable
				while (SQLite.step(stmHandle) == SQLite.ROW)
				{
					object[] row = new object[columnCount];
					for (int i = 0; i < columnCount; i++)
					{
						switch (SQLite.columnType(stmHandle, i))
						{
							case SQLite.INTEGER:
								row[i] = SQLite.columnInt(stmHandle, i);
								break;
							case SQLite.TEXT:
								row[i] = Marshal.PtrToStringUni(SQLite.columnText(stmHandle, i));
								break;
							case SQLite.FLOAT:
								row[i] = SQLite.columnDouble(stmHandle, i);
								break;
						}
					}

					dTable.Rows.Add(row);
				}

				return dTable;
			}
			finally
			{
				Finalize(stmHandle);
			}
		}

		#region ORM

		public IEnumerable<TypeT> queryColumns<TypeT>(string query)
		{
			// we can not use value types, because we can not set field members of them right now.
			if (typeof(TypeT).IsValueType)
				throw new SQLiteException("Can't use value types for queryColumns()");

			this.T("query type {0}: {1} ".format(typeof(TypeT).Name, query));

			//prepare the statement
			IntPtr stmHandle = Prepare(query);

			try
			{
				int columnCount = SQLite.columnCount(stmHandle);
				int expectedColumns = ORM<TypeT>.Columns.Length;
				if (columnCount != expectedColumns)
					throw new SQLiteException("Expected {0} columns for type {1}, but got {2}".format(expectedColumns, typeof(TypeT).Name, columnCount));


				//populate datatable
				while (SQLite.step(stmHandle) == SQLite.ROW)
				{
					var t = ORM<TypeT>.createInstance();

					for (int columnIndex = 0; columnIndex != columnCount; ++columnIndex)
					{
						copyColumnValueToField(stmHandle, columnIndex, t, ORM<TypeT>.Fields[columnIndex]);
					}

					yield return t;
				}
			}
			finally
			{
				Finalize(stmHandle);
			}
		}


		void copyColumnValueToField(IntPtr handle, int columnIndex, object targetObject, FieldInfo field)
		{
			var type = field.FieldType;
			Func<ColumnQueryParam, object> ff = FieldReader.resolve(type);
			var obj = ff(new ColumnQueryParam(_serializer, handle, columnIndex));
			field.SetValue(targetObject, obj);
		}

		#endregion

		#region Faster, Convenient Enumerables for querying single columns

		public IEnumerable<TypeT> querySingleColumn<TypeT>(string query)
		{
			return querySingleColumn(query, TypeDependentMethods<TypeT>.sqlite3_column);
		}

		public IEnumerable<TypeT> querySingleColumn<TypeT>(string query, Func<ColumnQueryParam, TypeT> retrieveColumnValue)
		{
			this.T(query);
			//prepare the statement
			IntPtr stmHandle = Prepare(query);

			try
			{
				int columnCount = SQLite.columnCount(stmHandle);
				if (columnCount != 1)
					throw new SQLiteException("Expected one column");

				//populate datatable
				while (SQLite.step(stmHandle) == SQLite.ROW)
					yield return retrieveColumnValue(new ColumnQueryParam(_serializer, stmHandle, 0));
			}
			finally
			{
				Finalize(stmHandle);
			}
		}


		public IEnumerable<Pair<RT1, RT2>> queryTwoColumns<RT1,RT2>(string query)
		{
			return queryTwoColumns(
				query, 
				TypeDependentMethods<RT1>.sqlite3_column, 
				TypeDependentMethods<RT2>.sqlite3_column);
		}

		public IEnumerable<Pair<RT1, RT2>> queryTwoColumns<RT1,RT2>(
			string query, 
			Func<ColumnQueryParam, RT1> retrieveColumnValue1, 
			Func<ColumnQueryParam, RT2> retrieveColumnValue2)
		{
			this.T(query);
			//prepare the statement
			IntPtr stmHandle = Prepare(query);

			try
			{
				int columnCount = SQLite.columnCount(stmHandle);
				if (columnCount != 2)
					throw new SQLiteException("Expected two columns");

				//populate datatable
				while (SQLite.step(stmHandle) == SQLite.ROW)
				{
					var value1 = retrieveColumnValue1(new ColumnQueryParam(_serializer, stmHandle, 0));
					var value2 = retrieveColumnValue2(new ColumnQueryParam(_serializer, stmHandle, 1));

					yield return Pair.make(value1, value2);
				}
			}
			finally
			{
				Finalize(stmHandle);
			}
		}


		#endregion


		#region Statement preparation / disposal

		/// <summary>
		/// Prepares a SQL statement for execution
		/// </summary>
		/// <param name="query">SQL query</param>
		/// <returns>Pointer to SQLite prepared statement</returns>
		private IntPtr Prepare(string query)
		{
			IntPtr stmHandle;

			if (SQLite.prepare(_db, query, query.Length<<1,
			  out stmHandle, IntPtr.Zero) != SQLite.OK)
				throw new SQLiteException(Marshal.PtrToStringUni(SQLite.errmsg(_db)));

			return stmHandle;
		}

		/// <summary>
		/// Finalizes a SQLite statement
		/// </summary>
		/// <param name="stmHandle">
		/// Pointer to SQLite prepared statement
		/// </param>
		private void Finalize(IntPtr stmHandle)
		{
			if (SQLite.finalize(stmHandle) != SQLite.OK)
				throw new SQLiteException(
				  "Could not finalize SQL statement: " + Marshal.PtrToStringUni(SQLite.errmsg(_db)));
		}

		#endregion
	}
}
