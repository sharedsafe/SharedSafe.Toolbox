using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Toolbox;
using Toolbox.Meta;
using System.Reflection.Emit;

namespace LibG4
{
	public abstract class TypeBuilder<MetaClassT>
		where MetaClassT : new()
	{
		public static readonly Type Meta = typeof(MetaClassT);
		static readonly ITypeResolver TypeResolver = new MetaClassT() as ITypeResolver;

		#region Interface

		/**
			Create an implemented property object based on a prototype interface.

			For now, the prototype interface is only allowed to contain Properties!
		**/

		public static InterfaceT make<InterfaceT>()
			where InterfaceT : class
		{
			var t = PropertyObject<InterfaceT>.Type;
			return (InterfaceT)TypeBuilderStatic.construct(Meta, t);
		}

		public static Interface1T make<Interface1T, Interface2T>()
		{
			var t = PropertyObject<Interface1T, Interface2T>.Type;
			return (Interface1T)TypeBuilderStatic.construct(Meta, t);
		}

		public static Interface1T make<Interface1T, Interface2T, Interface3T>()
		{
			var t = PropertyObject<Interface1T, Interface2T, Interface3T>.Type;
			return (Interface1T)TypeBuilderStatic.construct(Meta, t);
		}

		/// Resolve an actual G4 generated type by a given name, the type
		/// is generated as requested.

		public static Type resolve(string name)
		{
			var tr = TypeResolver;
			if (tr == null)
				throw new Exception("Unable to resolve type {0} in for type set {1}".format(name, Meta.Name));

			var types = tr.resolve(name);
			return TypeBuilderStatic.resolveMixin(Meta, types);
		}

		#endregion

		/// This wrapper implements a statically compile time bound type map!

		public static class PropertyObject<InterfaceT>
		{
			public static readonly Type Type = TypeBuilderStatic.resolveMixin(Meta, new[] { typeof(InterfaceT) });
		}

		// Note: for more than one type we introduce another indirection to return the same types 
		// if Interfaces are requested in a different order ;)

		public static class PropertyObject<Interface1T, Interface2T>
		{
			public static readonly Type Type = TypeBuilderStatic.resolveMixin(Meta, new[] { typeof(Interface1T), typeof(Interface2T) });
		}

		public static class PropertyObject<Interface1T, Interface2T, Interface3T>
		{
			public static readonly Type Type = TypeBuilderStatic.resolveMixin(Meta, new[] { typeof(Interface1T), typeof(Interface2T), typeof(Interface3T) });
		}


	}

	/**
		Static, non-generic helper component of the type builder.
	**/

	static class TypeBuilderStatic
	{
		static readonly object _lock = new object();
		static readonly Dictionary<Type, ModuleBuilder> Modules = new Dictionary<Type, ModuleBuilder>();
		static readonly Dictionary<string, Type> Mixins = new Dictionary<string, Type>();

		public static Type resolveMixin(Type meta, IEnumerable<Type> interfaces)
		{
			var tn = Helpers.makeTypeNameFromInterfaces(interfaces);
			Type t;

			lock (_lock)
			{
				if (Mixins.TryGetValue(tn, out t))
					return t;

				t = makeType(meta, interfaces);
				Mixins[tn] = t;
			}

			return t;
		}

		public static object make(Type meta, IEnumerable<Type> interfaces)
		{
			Type actual = resolveMixin(meta, interfaces);
			return construct(meta, actual);
		}

		public static object construct(Type meta, Type type)
		{
			return FormatterServices.GetUninitializedObject(type);
		}

		sealed class PropertyData
		{
			public Type Type;
			public bool Reader;
			public bool Writer;
			public bool Optional;

			public bool RequiredNonValueType
			{
				get
				{
					return !Optional && !Type.IsValueType;
				}
			}
		}

		// todo: we should pre-create all required interfaces!
		// so we should tag them as property-interface?

		static Type makeType(Type meta, IEnumerable<Type> interfaces)
		{
			var typeName = Helpers.makeTypeNameFromInterfaces(interfaces);

			var module = resolveModule(meta);

			TypeBuilder builder = module.DefineType(
				typeName,
				TypeAttributes.Public,
				typeof(ValueContainer));

			foreach (var i in interfaces)
				builder.AddInterfaceImplementation(i);

			// resolve all interfaces from the set of specified ones.
			interfaces = Helpers.resolveAllInterfaces(interfaces);

			var properties = new Dictionary<string, PropertyData>();


			foreach (var i in interfaces)
			{
				// temporarily removed!
				if (i.IsNotPublic)
					throw new Exception("Interface {0} must be public to be used in G4".format(i.FullName));

				foreach (var property in i.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					PropertyData prop;
					if (!properties.TryGetValue(property.Name, out prop))
					{

						prop = new PropertyData
						{
							Type = property.PropertyType,
						};

						properties[property.Name] = prop;
					}
					else if (!prop.Type.Equals(property.PropertyType))
						throw new Exception(
							"Cannot create mix of property {0}, detected duplicate within different interfaces {1}".format(
								property.Name, i.FullName));

					prop.Reader |= property.CanRead;
					prop.Writer |= property.CanWrite;
					prop.Optional |= property.hasAttribute<OptionalAttribute>();

					if (!prop.Optional)
						continue;

					if (prop.Type.IsValueType)
						throw new Exception("[Optional] can not be used on value types in {0}.{1}".format(i.FullName, property.Name));
				}
			}

			var fields = new FieldBuilder[properties.Count];
			uint index = 0;

			bool needConstructor = false;

			foreach (var property in properties)
			{
				fields[index] = buildProperty(property.Key, property.Value, builder, index, meta);
				++index;

				if (!needConstructor)
					needConstructor = !property.Value.Optional && !property.Value.Type.IsValueType;
			}

#if false
			if (needConstructor)
				buildConstructor(builder, properties, fields);
#endif

			return builder.CreateType();
		}


#if false
		static void buildConstructor(TypeBuilder builder, IEnumerable<KeyValuePair<string, PropertyData>> properties, FieldBuilder[] fields)
		{
			var constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			var il = constructor.GetILGenerator();

			var index = 0;

			foreach (var property in properties)
			{
				var field = fields[index++];

				// todo: field shall not be null (which means that there is no getter / setter specified, which cannot happen, or?
				if (field == null || !property.Value.Optional)
					continue;
				var type = field.FieldType;
				var tConstructor = type.GetConstructor(Type.EmptyTypes);
				if (tConstructor == null)
					throw new Exception(
						"Failed to resolve default parameterless constructor for type {0} to create default initializer for {1}.{2}"
							.format(type.Name, builder.Name, property.Key));

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, tConstructor);
				il.Emit(OpCodes.Stfld, field);
			}

			il.Emit(OpCodes.Ret);
		}

#endif

		static FieldBuilder buildProperty(string propertyName, PropertyData property, TypeBuilder builder, uint index, Type meta)
		{
			var propertyType = property.Type;
			var canRead = property.Reader;
			var canWrite = property.Writer;

			var propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

			FieldBuilder slot = null;

			if (canRead || canWrite)
			{
				var slotType = typeof(Slot<>).MakeGenericType(propertyType);
				slot = builder.DefineField("_" + propertyName, slotType, FieldAttributes.Private);
			}

			if (canRead)
			{
				var method = buildReader(slot, propertyName, propertyType, builder, index, property.RequiredNonValueType, meta);
				propertyBuilder.SetGetMethod(method);
			}

			// new: we always generate the writer for a property, so that a underlying serialization engine 
			// (such as JSonExSerializer) can access it.
			// if (canWrite)
			{
				var method = buildWriter(slot, propertyName, propertyType, builder, index);
				propertyBuilder.SetSetMethod(method);
			}

			return slot;
		}

		const MethodAttributes PropertyMethodAttributes =
			MethodAttributes.Public |
			MethodAttributes.SpecialName |
			MethodAttributes.HideBySig |
			MethodAttributes.Virtual |
			MethodAttributes.NewSlot |
			MethodAttributes.Final;

		static MethodBuilder buildReader(
			FieldInfo slot, 
			string name, 
			Type type, 
			TypeBuilder builder, 
			uint index, 
			bool requiredNonValueType,
			Type meta)
		{
			var method = builder.DefineMethod(
				"get_" + name,
				PropertyMethodAttributes,
				type,
				Type.EmptyTypes);

			var il = method.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldflda, slot);
			il.emitConst(index);
			il.Emit(OpCodes.Call, requiredNonValueType ? ValueContainer.makeGetPropertyRequired(type, meta) : ValueContainer.makeGetProperty(type));
			il.Emit(OpCodes.Ret);

			return method;
		}

		static MethodBuilder buildWriter(FieldInfo slot, string name, Type type, TypeBuilder builder, uint index)
		{
			var method = builder.DefineMethod(
				"set_" + name,
				PropertyMethodAttributes,
				null,
				new[] { type });

			var il = method.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldflda, slot);
			il.emitConst(index);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, ValueContainer.makeSetProperty(type));
			il.Emit(OpCodes.Ret);
			return method;
		}

#if false

		public static void savePropertyTypesAssembly()
		{
#if DEBUG
			var assembly = (AssemblyBuilder)PropertyTypesModule.Assembly;

			assembly.Save(assembly.GetName().Name + ".dll");
#endif
		}
#endif


		public static ModuleBuilder resolveModule(Type metaType)
		{
			ModuleBuilder mb;

			lock (_lock)
			{
				if (!Modules.TryGetValue(metaType, out mb))
				{
					mb = makeModule(metaType.Name);
					Modules[metaType] = mb;
				}
			}

			return mb;
		}

		static ModuleBuilder makeModule(string name)
		{
			var assemblyName = new AssemblyName(name);

#if DEBUG
			var access = AssemblyBuilderAccess.RunAndSave;
#else
			var access = AssemblyBuilderAccess.Run;
#endif
			var domain = AppDomain.CurrentDomain;
			var assembly = domain.DefineDynamicAssembly(assemblyName, access);
#if DEBUG
			var filename = assemblyName.Name + ".dll";
			var module = assembly.DefineDynamicModule(
				assemblyName.Name,
				filename,
				true);
#else
			var module = assembly.DefineDynamicModule(assemblyName.Name);
#endif
			return module;
		}
	}

	static class Helpers
	{
		public static void emitConst(this ILGenerator il, uint value)
		{
			if (value <= 8)
			{
				il.Emit(LdcInt[value]);
				return;
			}

			if (value <= 127)
			{
				il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
				return;
			}

			il.Emit(OpCodes.Ldc_I4, value);
		}

		static readonly OpCode[] LdcInt = {
		                                   	OpCodes.Ldc_I4_0,
		                                   	OpCodes.Ldc_I4_1,
		                                   	OpCodes.Ldc_I4_2,
		                                   	OpCodes.Ldc_I4_3,
		                                   	OpCodes.Ldc_I4_4,
		                                   	OpCodes.Ldc_I4_5,
		                                   	OpCodes.Ldc_I4_6,
		                                   	OpCodes.Ldc_I4_7,
		                                   	OpCodes.Ldc_I4_8,
		                                   };

		public static string makeTypeNameFromInterfaces(IEnumerable<Type> interfaces)
		{
			// need to sort names, so that TypeName of IFace1 IFace2 results the same as IFace2 IFace 1
			var names = from i in interfaces
						select makeTypeNameFromInterfaceName(i.Name)
							into n
							orderby n
							select n;

			return string.Join("_", names.ToArray());
		}

		public static string makeTypeNameFromInterfaceName(string interfaceName)
		{
			return interfaceName.StartsWith("I") ? interfaceName.Substring(1) : interfaceName;
		}


		/// Create a closure

		public static IEnumerable<Type> resolveAllInterfaces(IEnumerable<Type> interfaces)
		{
			var processed = new HashSet<Type>();
			internalResolveAllInterfaces(processed, interfaces);
			return processed;
		}

		static void internalResolveAllInterfaces(ICollection<Type> set, IEnumerable<Type> interfaces)
		{
			foreach (var i in interfaces)
			{
				if (set.Contains(i))
					continue;

				set.Add(i);
				internalResolveAllInterfaces(set, i.GetInterfaces());
			}
		}
	}

}
