using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RootSE.ORM;
using Toolbox;
using Toolbox.Constraints;
using Toolbox.Meta;

namespace RootSE.Provider
{
	/**
		Maps a return from SQL to a type's properties.
	**/

	[Flags]
	enum FieldFlags
	{
		Index = 0x01,
		Unique = 0x02,
		RowId = 0x04,
		Nullable = 0x08
	}

	[Pure]
	sealed class ORM<TypeT>
	{
		public static TypeT createInstance()
		{
			return (TypeT)Activator.CreateInstance(typeof (TypeT));
		}

		public static readonly string TableName = makeTableName();
		public static readonly FieldInfo[] Fields = makeFields();
		public static readonly FieldFlags[] Flags = makeFlags();
		public static readonly string[] ColumnNames = makeColumnNames();
		public static readonly Column[] Columns = makeColumns();

		public static readonly Index[] Indices = makeIndices();
		public static readonly Action<TypeT, ulong> RowIdSetter_ = tryMakeRowIdSetter();
		public static readonly int? PrimaryKeyIndex = makePrimaryKeyIndex();
		public static readonly string PrimaryKeyColumnName_ = makePrimaryKeyColumnName();

		static string makeTableName()
		{
			return plurify(typeof(TypeT).Name);
		}

		static string plurify(string f)
		{
			return f + "s";
		}

		static FieldInfo[] makeFields()
		{
			return typeof(TypeT).GetFields(BindingFlags.Public | BindingFlags.Instance);
		}

		static FieldFlags[] makeFlags()
		{
			var flags = new FieldFlags[Fields.Length];
			for (int i = 0; i != Fields.Length; ++i)
				flags[i] = makeFlags(Fields[i]);

			return flags;
		}

		static FieldFlags makeFlags(MemberInfo info)
		{
			var f = new FieldFlags();

			if (info.hasAttribute<IndexAttribute>())
				f |= FieldFlags.Index;

			if (info.hasAttribute<UniqueAttribute>())
				f |= FieldFlags.Unique;

			if (info.hasAttribute<RowIdAttribute>())
				f |= FieldFlags.RowId;

			if (info.hasAttribute<NullAttribute>())
				f |= FieldFlags.Nullable;

			return f;
		}


		static string[] makeColumnNames()
		{
			return (from f in Fields select f.Name).ToArray();
		}

		#region Columns

		static Column[] makeColumns()
		{
			var cnt = Fields.Length;

			var columns = new Column[cnt];

			for (int i = 0; i != cnt; ++i)
			{
				columns[i] = fieldToColumn(i);
			}

			return columns;
		}

		static Column fieldToColumn(int i)
		{
			var field = Fields[i];
			var flags = Flags[i];

			bool nullable = (flags & FieldFlags.Nullable) != 0;
			bool rowId = (flags & FieldFlags.RowId) != 0;

			if (rowId)
				nullable = true;

			return new Column(field.Name, Datatypes.toSQL(field.FieldType), !nullable, rowId);
		}

		#endregion

		#region Indices

		static Index[] makeIndices()
		{
			var _indices = new List<Index>();

			for (int i = 0; i != Fields.Length; ++i)
			{
				if ((Flags[i] & FieldFlags.Index) == 0)
					continue;

				bool unique = ((Flags[i] & FieldFlags.Unique) != 0);
				var index = new Index(new[] {ColumnNames[i]}, unique);
				_indices.Add(index);
			}

			return _indices.ToArray();
		}

		#endregion

		#region Setters

		static Action<TypeT, ulong> tryMakeRowIdSetter()
		{
			var index = tryGetRowIdFieldIndex();
			if (index == null)
				return null;

			return (instance, rowId) => Fields[index.Value].SetValue(instance, rowId);
		}

		static int? tryGetRowIdFieldIndex()
		{
			foreach (var fi in Flags.indices())
			{
				var flags = Flags[fi];
				if ((flags & FieldFlags.RowId) != 0)
				{
					return fi;
				}
			}

			return null;
		}



		#endregion

		#region Primary Index / Key

		static int? makePrimaryKeyIndex()
		{
			// row id is the preferred primary key

			var candidates = getCandidates(isRowId);
			if (candidates.Second > 1)
				throw new Exception("Unsuported: more than one row index specified for type {0}".format(typeof(TypeT).Name));

			if (candidates.Second == 0)
			{
				candidates = getCandidates(isUniqueIndex);
				if (candidates.Second > 1)
					throw new Exception("Unsupported: more than one unique index specified for type {0}".format(typeof(TypeT).Name));
			}

			if (candidates.Second == 0)
				return null;

			var index = candidates.First.Select(Pair.make);
			var fst = index.First(p => p.First);
			return fst.Second;
		}

		static Pair<bool[], int> getCandidates(Func<FieldFlags, bool> filter)
		{
			var first = Flags.Select(filter).ToArray();
			var second = first.Where(b => b).Count();
			return Pair.make(first, second);
		}

		static bool isUniqueIndex(FieldFlags flags)
		{
			bool isUniqueIndex = (flags & (FieldFlags.Index | FieldFlags.Unique)) == (FieldFlags.Index | FieldFlags.Unique);
			return isUniqueIndex;
		}

		static bool isRowId(FieldFlags flags)
		{
			return (flags & FieldFlags.RowId) != 0;
		}

		static string makePrimaryKeyColumnName()
		{
			var index = PrimaryKeyIndex;
			return index == null ? null : ColumnNames[index.Value];
		}

		#endregion
	}
}
