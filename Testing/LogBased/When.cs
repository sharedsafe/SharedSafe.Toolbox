using NUnit.Framework.Constraints;
using Toolbox.Testing.LogBased.Detail;
using Toolbox.Testing.LogBased.Detail.Triggers;

namespace Toolbox.Testing.LogBased
{
	public static class When
	{
		public static Trigger Message(ILogMessageFilter filter, Constraint textConstraint)
		{
			var combined = Combine.FilterWithConstraint(filter, textConstraint);

			return new InterceptorTrigger(combined);
		}
	}
}
