namespace RootSE.Provider
{
	public sealed class OrderBy
	{
		public OrderBy(IExpression expression, bool descending)
		{
			_expression = expression;
			_descending = descending;
		}

		readonly IExpression _expression;
		readonly bool _descending;

		public string SQL
		{
			get { return _expression.SQL + (_descending ? " desc" : ""); }
		}

		public OrderBy Descending
		{
			get { return new OrderBy(_expression, true); }
		}

		public static OrderBy column(string columnName)
		{
			return new OrderBy(Term.column(columnName), false);
		}
	}
}
