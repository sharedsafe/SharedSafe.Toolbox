using System;
using System.Collections.Generic;
using System.Reflection;
using Toolbox;

namespace LibG4
{
	public abstract class ValueContainer
	{
// ReSharper disable MemberCanBeMadeStatic
		protected ValueT getProperty<ValueT>(ref Slot<ValueT> slot, uint propertyIndex)
// ReSharper restore MemberCanBeMadeStatic
		{
			var transaction = TransactionManager.CurrentTransaction;
			if (transaction != null)
			{
				if (slot.Readers == null)
					slot.Readers = new List<Transaction>();

				transaction.notifyRead(slot.Readers);
			}

			return slot.Value;
		}

		protected ValueT getPropertyRequired<ValueT>(ref Slot<ValueT> slot, uint propertyIndex)
			where ValueT : class, new()
		{
			if (slot.Value == null)
				slot.Value = new ValueT();

			return getProperty(ref slot, propertyIndex);
		}

		protected string getPropertyRequiredString(ref Slot<string> slot, uint propertyIndex)
		{
			if (slot.Value == null)
				slot.Value = string.Empty;

			return getProperty(ref slot, propertyIndex);
		}

		protected ElementT[] getPropertyRequiredArray<ElementT>(ref Slot<ElementT[]> slot, uint propertyIndex)
		{
			if (slot.Value == null)
				slot.Value = Array<ElementT>.Empty;

			return getProperty(ref slot, propertyIndex);
		}

		protected InterfaceT getPropertyRequiredInterface<InterfaceT, MetaT>(ref Slot<InterfaceT> slot, uint propertyIndex)
			where InterfaceT : class
		{
			if (slot.Value == null)
				slot.Value = (InterfaceT)TypeBuilderStatic.make(typeof(MetaT), typeof (InterfaceT).asEnumerable());

			return getProperty(ref slot, propertyIndex);
		}

// ReSharper disable MemberCanBeMadeStatic
		protected void setProperty<ValueT>(ref Slot<ValueT> slot, uint propertyIndex, ValueT value)
// ReSharper restore MemberCanBeMadeStatic
		{
			// note: this will box the value
			// we may need to put this in ilcode or provide different setProperty Methods (one for value types and 
			// another for reference types)

			// alternative: require IEquatable<> to be implemented by all value types!!!!!

			if (Equals(value, slot.Value))
				return;

			slot.Value = value;

			if (slot.Readers != null)
				slot.Readers.ForEach(t => t.notifyWrite());
		}

		static readonly Type ValueContainerType = typeof (ValueContainer);

		static MethodInfo resolveMethod(string name)
		{
			return ValueContainerType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		static readonly MethodInfo GetProperty = resolveMethod("getProperty");
		static readonly MethodInfo SetProperty = resolveMethod("setProperty");

		static readonly MethodInfo GetPropertyRequired = resolveMethod("getPropertyRequired");
		static readonly MethodInfo GetPropertyRequiredString = resolveMethod("getPropertyRequiredString");
		static readonly MethodInfo GetPropertyRequiredInterface = resolveMethod("getPropertyRequiredInterface");
		static readonly MethodInfo GetPropertyRequiredArray = resolveMethod("getPropertyRequiredArray");
		/**
			Note, todo: be sure that same type instances share the same MethodInfos!!!1
		**/
		
		public static MethodInfo makeGetProperty(Type propertyType)
		{
			return GetProperty.MakeGenericMethod(propertyType);
		}

		public static MethodInfo makeSetProperty(Type propertyType)
		{
			return SetProperty.MakeGenericMethod(propertyType);
		}

		public static MethodInfo makeGetPropertyRequired(Type propertyType, Type meta)
		{
			if (propertyType.Equals(StringType))
				return GetPropertyRequiredString;

			if (propertyType.IsInterface)
				return GetPropertyRequiredInterface.MakeGenericMethod(propertyType, meta);

			if (propertyType.IsArray)
				return GetPropertyRequiredArray.MakeGenericMethod(propertyType.GetElementType());

			return GetPropertyRequired.MakeGenericMethod(propertyType);
		}

		static readonly Type StringType = typeof(string);
	}
}
