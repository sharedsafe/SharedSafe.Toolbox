#if WPF
using System;
using System.Linq.Expressions;
using System.Windows;

namespace Toolbox.WPF
{
	public struct Property<ValueT>
	{
		Property(DependencyProperty prop)
		{
			_depProp = prop;
		}

		readonly DependencyProperty _depProp;

		public static implicit operator Property<ValueT>(PropertyBase prop)
		{
			return new Property<ValueT>(prop.DependencyProperty);
		}

		public static implicit operator DependencyProperty(Property<ValueT> prop)
		{
			return prop._depProp;
		}

		public ValueT this[DependencyObject obj]
		{
			get { return (ValueT)obj.GetValue(_depProp); }
			set { obj.SetValue(_depProp, value); }
		}
	}

	public abstract class PropertyBase
	{
		public readonly DependencyProperty DependencyProperty;

		protected PropertyBase(DependencyProperty prop)
		{
			DependencyProperty = prop;
		}
	}

	public sealed class Property<ContainerT, ValueT> : PropertyBase
	{
		public Property(Expression<Func<ContainerT, ValueT>> accessor)
			: base(DependencyProperty.Register(nameOfMember(accessor), typeof(ValueT), typeof(ContainerT)))
		{
		}

		static string nameOfMember(Expression<Func<ContainerT, ValueT>> expression)
		{
			var memberexp = expression.Body as MemberExpression;
			if (memberexp == null)
				throw new ArgumentException("Failed to get name of expression member", "expression");
			return memberexp.Member.Name;
		}
	}
}
#endif