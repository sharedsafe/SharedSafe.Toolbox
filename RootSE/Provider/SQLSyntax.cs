namespace RootSE.Provider
{
	static class SQLSyntax
	{
		public static string equalValueExpression(string columnName, object value)
		{
			var escapedColumn = Escape.column(columnName);

			return value == null
				? escapedColumn + " ISNULL"
				: escapedColumn + "=" + ValueEncoder.encode(value, null);
		}
	}
}
