using System;
using System.Collections.Generic;
using System.Reflection;

namespace Toolbox
{
	public static class ObjectExtensions
	{
		public static IEnumerable<ObjT> asEnumerable<ObjT>(this ObjT obj)
		{
			yield return obj;
		}

		public static ObjT cast<ObjT>(this object obj)
		{
			return (ObjT) obj;
		}

		public static OT memberwiseClone<OT>(this OT obj)
		{
			return (OT)MemberwiseClone(obj);
		}

		static new readonly Func<object, object> MemberwiseClone = makeMC();

		static Func<object, object> makeMC()
		{
			var m = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
			return (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), m);
		}
	}
}
