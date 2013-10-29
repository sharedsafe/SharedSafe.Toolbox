using System;
using System.Linq;
using Toolbox;

namespace RootSE.Provider
{
	static class Escape
	{
		public static string qualifiedColumn(string t, string c)
		{
			return table(t) + "." + column(c);
		}

		public static string table(string table)
		{
			return table.Any(c => (c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_') 
				? bracketTableName(table) 
				: table;
		}

		// I am not sure if this is valid anymore!

		static string bracketTableName(string table)
		{
			if (table.IndexOfAny(BracketForbidden) != -1)
				throw new Exception("Table name {0} invalid".format(table));

			return '[' + table + ']';
		}

		static readonly char[] BracketForbidden = {'[', ']'};

		public static string column(string columnName)
		{
			return identifier(columnName);
		}

		public static string[] columns(string [] columnNames)
		{
			return columnNames.Select(column).ToArray();
		}

		public static string index(string indexName)
		{
			return identifier(indexName);
		}

		static string identifier(string identifierName)
		{
			return '"' + identifierName.Replace("\"", "\"\"") + '"';
		}
	}
}
