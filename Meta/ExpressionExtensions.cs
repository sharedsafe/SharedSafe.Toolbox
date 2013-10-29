using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Toolbox.Meta
{
	public static class ExpressionExtensions
	{
		public static Action<A, B> makeSetter<A, B>(this Expression<Func<A, B>> expression)
		{
			var memberexp = expression.Body as MemberExpression;
			if (memberexp == null)
				throw new InternalError("Failed to derive setter from Lambda Expression");
			var valueParameter = Expression.Parameter(typeof(B), "value");
			if (memberexp.Member is FieldInfo)
			{
				var setCall = Expression.Call(typeof(ExpressionExtensions),
					"setField",
					new[] { typeof(B) },
					memberexp,
					valueParameter);
				var exp = Expression.Lambda(setCall, expression.Parameters[0], valueParameter);
				return (Action<A, B>)exp.Compile();
			}

			var pi = memberexp.Member as PropertyInfo;

			if (pi != null)
			{
				// todo: make this somehow faster (can't we extract the property setter in a direct way from
				// propertyinfo?)

				var piParameter = Expression.Parameter(typeof(PropertyInfo), "pi");

				var setCall = Expression.Call(typeof(ExpressionExtensions),
					"setProperty",
					new[] { memberexp.Expression.Type, typeof(B) },
					memberexp.Expression,
					valueParameter,
					piParameter);

				var exp = Expression.Lambda(setCall, expression.Parameters[0], valueParameter, piParameter);
				return (a, b) => ((Action<A, B, PropertyInfo>)exp.Compile())(a, b, pi);
			}
			
			throw new InternalError("Failed to derive setter from Lambda Expression, no field or no property");
		}

		public static void setField<T>(ref T t, T value)
		{
			t = value;
		}

		public static void setProperty<C, T>(C c, T value, PropertyInfo pi)
		{
			pi.SetValue(c, value, null);
		}

		public static string nameOfMember<A, B>(this Expression<Func<A, B>> expression)
		{
			var memberexp = expression.Body as MemberExpression;
			if (memberexp == null)
				throw new InternalError("Failed get name of expression member");
			return memberexp.Member.Name;
		}

		public static string tryGetNameOfMember<A, B>(this Expression<Func<A, B>> expression)
		{
			var memberexp = expression.Body as MemberExpression;
			if (memberexp == null)
				return null;
			return memberexp.Member.Name;
		}
	}
}
