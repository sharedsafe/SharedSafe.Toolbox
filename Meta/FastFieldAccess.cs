using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// http://forums.msdn.microsoft.com/en-US/netfxbcl/thread/03bd3d83-dcb0-4231-a1f7-4ca487fc5535/

#if false
namespace Toolbox.Meta
{
	Code Block

delegate GetFieldCall(object target);

 

GetFieldCall GetFieldAccessDelegate(FieldInfo field) {

 

    DynamicMethod dm = ...;

    ILGenerator g = dm.GetILGenerator();

 

    if(!field.IsStatic) {

     
http://www.codeproject.com/KB/cs/DynamicCodeVsReflection.aspx
CodeProject: Fast Dynamic Property/Field Accessors. Free source code and programming help
        generator.Emit(OpCodes.Ldarg_0);

         

        if(field.DeclaringType.IsValueType) {

            generator.Emit(OpCodes.Unbox,field.DeclaringType);

        } else {

            generator.Emit(OpCodes.Castclass,field.DeclaringType);

        }

         

        generator.Emit(OpCodes.Ldfld,field);

     

    }else {

        generator.Emit(OpCodes.Ldsfld,field);

    }

     

    if(field.FieldType.IsValueType) {

        generator.Emit(OpCodes.Box,field.FieldType);

    }

     

    generator.Emit(OpCodes.Ret);

     

}
	}
}

#endif
