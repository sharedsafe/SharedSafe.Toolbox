namespace RootSE.Provider
{
	public struct ColumnValue
	{
		public readonly string Column;
		public readonly object Value;

		public ColumnValue(string column, object value)
		{
			Column = column;
			Value = value;
		}
	}
}
