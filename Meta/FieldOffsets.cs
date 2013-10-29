/**
	Provides a table of field offsets from a particular type.

	The fields and the field offsets do only represent the fields in the type, not 
	in one if its base classes.
**/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace Toolbox.Meta
{
	public static class FieldOffsets<TypeT>
	{
		/// The type we are talking about
		public static readonly Type Type = typeof (TypeT);

		/// All the fields of that type.
		public static readonly FieldInfo[] Fields = Type.GetFields(
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic | 
			BindingFlags.DeclaredOnly);

		/// All the offsets of the fields of that type.
		public static readonly int[] Offsets = FieldOffset.resolve(Type, Fields);
	}

	/// A wrapper around our offset calculation code.  The actual offset calculation code is
	/// in a dynamically generated assembly, this class is just a wrapper to make it easier to call.
	///
	/// Main job of this class is to ensure that we generate the dynamic assembly exactly once</remarks>

	public static class FieldOffset
	{
		/// Find the position, in memory, of a field relative to the object that contains it.
		/// It is up to the caller to make sure to pass a <paramref name="field"/> that really is in <paramref name="containingObject"/>
		
		public static int get<FieldT>(object containingObject, ref FieldT field)
		{
			return OffsetInvoker<FieldT>.getOffset(containingObject, ref field);
		}

		#region Offset Creation Method, inspired by ActiveSharp project

		public static int[] resolve(Type type, FieldInfo[] fields)
		{
			var dynamicMethod = new DynamicMethod(
				"makeOffsets",
				typeof(void),
				new[] { typeof(object), typeof(int[]) },
				type, // owner
				false);

			ILGenerator gen = dynamicMethod.GetILGenerator();

			MethodInfo offsetMethod =
				typeof(FieldOffset).GetMethod("get", BindingFlags.Static | BindingFlags.Public);

			// for each field in it, get it offset then shove the result into the dictionary (dictionary of field names, keyed by offset)


			for (var i = 0; i != fields.Length; ++i)
			{
				// Generate the MSIL to get the offset for the given field. Here the indenting convention is that, for each call, the lines that put its arguments on the stack are indented one step relative to the line with the call
				// Note that this generated MSIL can access PRIVATE fields, simply because it is a Lightweight Code-gen method (and assuming appropriate permissions are granted)
				// See http://blogs.msdn.com/joelpob/archive/2004/03/31/105282.aspx 
				//and http://blogs.msdn.com/shawnfa/archive/2006/10/05/Using-Lightweight-CodeGen-from-Partial-Trust.aspx

				MethodInfo typedOffsetMethod = offsetMethod.MakeGenericMethod(fields[i].FieldType);

				// prepare setting the array element

				gen.Emit(OpCodes.Ldarg_1); // push array on stack
				gen.Emit(OpCodes.Ldc_I4, i); // push offset on stack

				// gen field offset

				gen.Emit(OpCodes.Ldarg_0); // object instance (for typedOffsetMethod)
				gen.Emit(OpCodes.Ldarg_0); // object instance (for Ldflda)
				gen.Emit(OpCodes.Ldflda, fields[i]); // field reference ref FieldT field.
				gen.Emit(OpCodes.Call, typedOffsetMethod);

				// value is uint, write it to the array
				gen.Emit(OpCodes.Stelem_I4);
			}

			// done
			gen.Emit(OpCodes.Ret);

			/// Create a delegate and call the method to retrieve the offsets!

			var method = (Action<object, int[]>)dynamicMethod.CreateDelegate(typeof(Action<object, int[]>));

			var instance = FormatterServices.GetSafeUninitializedObject(type);
			var offsets = new int[fields.Length];
			method(instance, offsets);
			return offsets;
		}

		#endregion
		
		
		
		#region implementation

		static readonly Type OffsetGetterType = makeOffsetGetterType();          // static, so that we make the dynamic assembly only once

		/// A helper method to help is invoke the generic method that we are targeting, while only
		/// resorting to reflection once (for each FieldT)
		///
		/// Main job of this class is to ensure that we generate the typed delegate exactly once for each FieldT</remarks>
	
		static class OffsetInvoker<T>
		{
			// static, so that we do the reflection, to create the delegate, just once for each FieldT
			static readonly OffsetGetter<T> GetOffset = (OffsetGetter<T>)Delegate.CreateDelegate(
				typeof(OffsetGetter<T>),
				null,
				OffsetGetterType.GetMethod("get").MakeGenericMethod(typeof(T)));

			public static int getOffset(object containingObject, ref T field)
			{
				return GetOffset(containingObject, ref field);
			}
		}

		#endregion
		delegate int OffsetGetter<FieldT>(object containingObject, ref FieldT field);


		/// Builds a method to get offsets.  This method cannot be coded directly in C# because it 
		/// requires features that are not exposed by the C# language.  Options are to write a separate assembly in
		/// C++ (since C++ can do what we need) or to generate the required assembly dynamically, as we are doing here.

		static Type makeOffsetGetterType()
		{
			#region Notes on how this works
			/*
         * 
         * 
        I originally used a C++ method like this:
         
        generic <typename FieldT, typename U>
		static int getOffset(FieldT% referencePoint, U% field)
		{
			pin_ptr<FieldT> pinnedReferencePoint = &referencePoint;
			Byte* rawReferencePoint = (Byte*)pinnedReferencePoint;

			pin_ptr<U> pinnedField = &field;
			Byte* rawFieldPointer = (Byte*)pinnedField;

			return rawFieldPointer - rawReferencePoint;
		}            
        
        It required TWO fields in the same object, and computed the distance between them. 
         
        I did consider writing the method in C# like this:
        
        public static unsafe int getOffset<FieldT, U>(FieldT containingObject, ref U field)
        {
            fixed (U* localRef2 = &containingObject)     // ***
            {
                byte* numPtr2 = localRef2;
                fixed (FieldT* localRef = field)              // ***
                {
                    byte* numPtr = localRef;
                    return (int) (numPtr - numPtr2);
                }
            }
        }
         
         * However the lines marked *** will not compile because(you can't assign arbitrary fields to pointers in C#)
         * Interestingly, using reflection.Emit to generate the equivalent code did work - i.e. it is the C# compiler that objects to 
         * the assignment not the jitter itself
         * 
         * Looking at the resulting MSIL, the only difference is that in the (Emit-generated) C#-style version, 
         * the pinned (fixed) pointers do not have this: modopt(IsExplicitlyDereferenced)
         * Given that modopt is in fact OPTIONAL for the JITter, and that the code still works without it, 
         * I'm going with the reflection-emit version for now.
         * 
         * Why? It removes the need to have a separate assembly to contain code written in C++.
         * 
         * What other options exist? 
         * (a) Separate assembly written in C++
         * (b) As for (a), but use post-build manipulation of assemblies to roll that separate assembly into this one
         * (c) Use DynamicILInfo to generate a synamic method that corresponds exactly to the C++ one.
        
         * Note: why is the reference point necessary?
         * It would have been nice to use the "this" pointer of the containing object, instead of having to
         * provide some other field from the object as a reference point.
         * However, neither C++ or C# allow us to get an unmanaged pointer to a "whole" object - which is what would be required in that case
         * Instead, all we can do it get pointers to members of objects.
         * See http://www.thescripts.com/forum/thread282166.html and http://msdn2.microsoft.com/en-us/library/ms235267(VS.80).aspx

         * However, I realised that the inability to get pointers to whole objects was imposed by the C++ and C# compilers,
         * not by the CLR itself.  (For instance, the older "managed extensions for c++" do let you get a pointer to a whole object.
         * 
         * It is hard to combine the managed extension approach with the code I want, because I want to use generics, which are not in the older Managed Extensions.
         * 
         * I.e. we want this code to deal with the first parameter:
         	__gc class Class1 // Written with the old "Managed Extensions"
            {
	        static void GetMemoryLocation(Object __gc* containingObject)
		        {
			        Object __pin* pinnedContainingObject = containingObject;
			        unsigned char * rawContainingObject = (unsigned char *)pinnedContainingObject;
         
         * and this code to deal with the second
         
        generic <typename FieldT, typename U>
		static int getOffset(... U% field)  // Written with the new "C++/CLI"
		{
			...

			pin_ptr<U> pinnedField = &field;
			Byte* rawFieldPointer = (Byte*)pinnedField;
		}   
        
        Note that both "unsigned char *" in Managed Extensions and "Byte*" in C++/CLI map to uint* in the complier-generated MSIL (and in the MSIL that we programatically generate here)

         * 
         * 
         * So, I have written raw MSIL, using Reflection.Emit, to produce the desired result.
         * The MSIL emitted is a combination of that produced by the two snippets above (one from 
         * Managed Extensions and one from C++/CLI).  As noted above, it omits the modopt(IsExplicitlyDereferenced)
         * from the pinned field's local variable.
         * 
        */

			#endregion

			// note: this routine defines a dynamic assembly, rather than using lightweight code gen, simply so that
			// you can save it to disk if you want, for debugging purposes.  (Or even to grab a copy of it and then change the code to link statically to it instead of generting it each time, if you really want)

			var assemblyName = new AssemblyName("ActiveSharp.OffsetFinder.Dynamic");
			AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
				assemblyName,
				AssemblyBuilderAccess.Save);  // todo: remove save access once we no loner need it

			ModuleBuilder myModule = dynamicAssembly.DefineDynamicModule(
				assemblyName.Name,
				assemblyName.Name + ".dll");

			TypeBuilder typeBuilder = myModule.DefineType("OffsetUtil", TypeAttributes.Public);

			// Declaring method builder
			MethodBuilder method = typeBuilder.DefineMethod("get",
				MethodAttributes.Public |
				MethodAttributes.Static |
				MethodAttributes.HideBySig);

			GenericTypeParameterBuilder[] genericParameters = method.DefineGenericParameters("T");
			genericParameters[0].SetGenericParameterAttributes(GenericParameterAttributes.None);

			// Preparing Reflection instances
			// Setting return type
			method.SetReturnType(typeof(Int32));
			// Adding parameters
			method.SetParameters(typeof(object), genericParameters[0].MakeByRefType());

			method.DefineParameter(1, ParameterAttributes.None, "obj");
			method.DefineParameter(2, ParameterAttributes.None, "field");

			ILGenerator gen = method.GetILGenerator();
			// Preparing locals

			// result of calculation - 0
			gen.DeclareLocal(typeof(Int32));

			// rawFieldPointer - 1
			gen.DeclareLocal(typeof(Byte*));

			// pinnedField - 2
			gen.DeclareLocal(genericParameters[0].MakeByRefType(), true);  // note use of pinned ref type here....

			// rawContainingObject - 3
			gen.DeclareLocal(typeof(Byte*));

			// pinnedContainingObjectPoint - 4
			gen.DeclareLocal(typeof(object), true);  // just a pinned object (that's what Managed Extensions for C++ uses at this point)

			// Writing body (thanks to the Reflection.Emit plug in for Integrator for helping with this!)

			// pin the containing object
			// gen.Emit(OpCodes.Ldnull);       // store null in the pinned object field... (as done in the code produced by Managed Extensions - I'm not 100% sure it is necessary)...
			// gen.Emit(OpCodes.Stloc_S, 4);

			gen.Emit(OpCodes.Ldarg_0);      //  then store the actual object into the pinned object field (again, as done in the code produced by Managed Extensions.  Even though C++/CLI and C# won't let us do this, the JIT seems to have no objection to doing so from this hand-coded MSIL)
			gen.Emit(OpCodes.Stloc_S, 4);

			// now, copy the reference to it into our byte pointer (to give us a "byte-sized" pointer that we can do arithmetic with it later)
			// Note that Managed Extensions generated conv.i then conv.i4.  I have replaced that with a single conv.ovf.u, which I believe is safe(r?) 
			gen.Emit(OpCodes.Ldloc_S, 4);
			gen.Emit(OpCodes.Conv_Ovf_U);    // now type Table 8: Conversion Operations, in the CLI Standard suggests that this conversion is (a) acceptable and (b) advisable (in that there is no documented support for doing without it, although it worked just fine in testing when I did omit this, as was the case in beta 1).  
			gen.Emit(OpCodes.Stloc_3);

			// * code above this point was prototyped in Managed Extensions,
			// * code below in C++/CLI

			// pin the field that we have been passed by reference, by copying it into a pinned local var
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stloc_2);

			// copy the reference to it into the other byte pointer (again, to get "byte-sized" pointers for the arithmetic we are about to do)
			gen.Emit(OpCodes.Ldloc_2);
			gen.Emit(OpCodes.Stloc_1);

			// subtract the two byte-sized pointers, to get the number of bytes between them, and return the result
			gen.Emit(OpCodes.Ldloc_1);
			gen.Emit(OpCodes.Ldloc_3);
			gen.Emit(OpCodes.Sub);
			gen.Emit(OpCodes.Stloc_0);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Ret);

			// finished
			return typeBuilder.CreateType();
		}
	}
}
