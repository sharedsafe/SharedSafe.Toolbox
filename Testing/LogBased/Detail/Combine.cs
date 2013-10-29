using NUnit.Framework.Constraints;

namespace Toolbox.Testing.LogBased.Detail
{
	static class Combine
	{
		public static ILogMessageFilter FilterWithConstraint(ILogMessageFilter filter, Constraint constraint)
		{
			var constraintFilter = new LogMessageFilter(msg => constraint.Matches(msg.Text));
			var combinedFilter = filter.chain(constraintFilter);

			return combinedFilter;
		}
	}
}
