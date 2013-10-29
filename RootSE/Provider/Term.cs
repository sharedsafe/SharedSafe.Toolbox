using Toolbox.Meta;
using System;
using System.Linq.Expressions;

namespace RootSE.Provider
{
	public abstract class Criteria
	{
		public abstract string SQL { get; }

		public static readonly Criteria All = new AllCriteria();
	}

	sealed class AllCriteria : Criteria
	{
		public override string SQL
		{
			get { return string.Empty; }
		}
	}

	public interface IExpression
	{
		string SQL { get; }
	}

	public sealed class ColumnExpression : IExpression
	{
		public string TableName_;
		public string ColumnName;

		public string SQL
		{
			get 
			{ 
				return TableName_ != null 
					? Escape.qualifiedColumn(TableName_, ColumnName) 
					: Escape.column(ColumnName); 
			}
		}

		public TermCriteria equals(object value)
		{
			return Term.binary(this, "=", Constant.value(value));
		}

		public TermCriteria equals(ColumnExpression column)
		{
			return Term.binary(this, "=", column);
		}

		public TermCriteria less(object value)
		{
			return Term.binary(this, "<", Constant.value(value));
		}

		public TermCriteria notEquals(object value)
		{
			var expression = new ConstantExpression {F = () => value};

			return new TermCriteria
			{
				Left = this,
				Operator = "<>",
				Right = expression
			};
		}
		public TermCriteria lessOrEqual(object value)
		{
			return Term.binary(this, "<=", Constant.value(value));
		}
		
		public TermCriteria greater(object value)
		{
			return Term.binary(this, ">", Constant.value(value));
		}

		public TermCriteria greaterOrEqual(object value)
		{
			return Term.binary(this, ">=", Constant.value(value));
		}
	}

	public sealed class TermCriteria : Criteria, IExpression
	{
		public IExpression Left;
		public string Operator;
		public IExpression Right;

		public override string SQL 
		{
			get
			{
				var op = Operator;
				var v = Right.SQL;
				if (op == "=" && v == "NULL")
					return Left.SQL + " IS NULL";
				return Left.SQL + " " + op + " " + v;
			}
		}

		public TermCriteria And(TermCriteria right)
		{
			return new TermCriteria()
			{
				Left = this,
				Operator = "AND",
				Right = right
			};
		}

		public TermCriteria Or(TermCriteria right)
		{
			return new TermCriteria()
			{
				Left = this,
				Operator = "OR",
				Right = right
			};
		}
	}

	public sealed class ConstantExpression : IExpression
	{
		public Func<object> F;

		public string SQL { get { return ValueEncoder.encode(F(), null); } }
	}

	public static class Constant
	{
		public static ConstantExpression value(object v)
		{
			return new ConstantExpression {F = () => v};
		}
	}

	sealed class ExpressionBuilder
	{
		public static ColumnExpression column<T, MT>(Expression<Func<T, MT>> expression)
		{
			return new ColumnExpression { ColumnName = expression.nameOfMember() };
		}
	}

	public static class Term
	{
		public static ColumnExpression column<T, MT>(Expression<Func<T, MT>> expression)
		{
			return column(expression.nameOfMember());
		}

		public static ColumnExpression column(string tableName, string columnName)
		{
			return new ColumnExpression { TableName_ = tableName, ColumnName = columnName };
		}

		public static ColumnExpression column(string columnName)
		{
			return new ColumnExpression { ColumnName = columnName };
		}

		public static TermCriteria binary(IExpression l, string op, IExpression r)
		{
			return new TermCriteria
			{
				Left = l,
				Operator = op,
				Right = r
			};
		}
	}
}