using System;

namespace DomainModeling
{
	/**
		is this even required here?
	**/

	public struct Reference
	{
		public readonly Type Type;
		public readonly Guid Id;

		internal Reference(Type type, Guid id)
		{
			Type = type;
			Id = id;
		}

		public static Reference of<TypeT>(Guid id)
		{
			return of(typeof(TypeT), id); 
		}

		public static Reference of(Type t, Guid id)
		{
			return new Reference(t, id);
		}

		public bool isOfType<TypeT>()
		{
			return isOfType(typeof (TypeT));
		}

		public bool isOfType(Type t)
		{
			return Type.Equals(t);
		}

		public override string ToString()
		{
			return Type.Name + ":" + Id;
		}

		#region R#

		public bool Equals(Reference other)
		{
			return Equals(other.Type, Type) && other.Id.Equals(Id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Reference))
				return false;
			return Equals((Reference) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Type.GetHashCode()*397) ^ Id.GetHashCode();
			}
		}

		public static bool operator ==(Reference left, Reference right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Reference left, Reference right)
		{
			return !left.Equals(right);
		}

		#endregion
	}

	public static class ReferenceExtensions
	{
		public static Reference toReference<TypeT>(this Guid guid)
		{
			return new Reference(typeof(TypeT), guid);
		}
	}
}
