using System;

namespace RootSE.Provider
{
	public struct Column
	{
		public readonly string Name;
		public readonly string Type;
		public readonly bool NotNull;
		public readonly bool PrimaryKey;

		public Column(string name, string type, bool notNull, bool primaryKey)
		{
			Name = name;
			Type = type;
			NotNull = notNull;
			PrimaryKey = primaryKey;
		}

		public string TypeAndConstraint
		{
			get { return Type + (NotNull ? " NOT NULL" : "") + (PrimaryKey ? " PRIMARY KEY" : ""); }
		}

		public bool Equals(Column other)
		{
			return Equals(other.Name, Name) && Equals(other.Type, Type) && other.NotNull.Equals(NotNull) && other.PrimaryKey.Equals(PrimaryKey);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Column))
				return false;
			return Equals((Column) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Name.GetHashCode();
				result = (result*397) ^ Type.GetHashCode();
				result = (result*397) ^ NotNull.GetHashCode();
				result = (result*397) ^ PrimaryKey.GetHashCode();
				return result;
			}
		}
	}
}
