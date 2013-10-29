/**
	A maybe type which can be used on reference _and_ value types.

	(we would use Nullable<FieldT> if value types would be possible).
**/

using System.Diagnostics;

namespace Toolbox
{
	public struct Maybe<ValueT>
	{
		public readonly bool HasValue;
		readonly ValueT _value;

		public ValueT Value
		{
			get
			{
				Debug.Assert(HasValue);
				return _value;
			}
		}

		public Maybe(ValueT value)
		{
			HasValue = true;
			_value = value;
		}

		public override bool Equals(object obj)
		{
			return obj is Maybe<ValueT> && this == (Maybe<ValueT>)obj;
		}

		public override int GetHashCode()
		{
			// don't think that it makes sense to eor the has code
			// of the true here

			return !HasValue ? HasValue.GetHashCode() : _value.GetHashCode();
		}

		public override string ToString()
		{
			return HasValue ? _value.ToString() : "invalid";
		}

		public static bool operator ==(Maybe<ValueT> l, Maybe<ValueT> r)
		{
			if (l.HasValue != r.HasValue)
				return false;
			return !l.HasValue || Equals(l.Value, r.Value);
		}

		public static bool operator !=(Maybe<ValueT> l, Maybe<ValueT> r)
		{
			return !(l == r);
		}

		/// Note: the implicit operator won't work, must use an extension method instead.
		/// For example returning null for a Maybe<string> would be allowed, which 
		/// is not what's intended (intended would be a return Nothing).

#if false

		public static implicit operator Maybe<ValueT>(ValueT value)
		{
			return new Maybe<ValueT>(value);
		}

#endif

		public static implicit operator Maybe<ValueT>(Nothing nothing)
		{
			return new Maybe<ValueT>();
		}
	}

	public struct Nothing
	{
	};

	public static class Maybe
	{
		public static Nothing Nothing;

		public static Maybe<TypeT> toMaybe<TypeT>(this TypeT value)
		{
			return new Maybe<TypeT>(value);
		}
	}
}
