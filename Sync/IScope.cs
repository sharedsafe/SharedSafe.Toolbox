using System.Text;

namespace Toolbox.Sync
{
	public interface IScope
	{
		string Name { get; }
		IScope Parent_ { get; }
		IScope enter(string name);
	}

	/**
		Public, users may use this to iterate over ISyncItems for example..
	**/

	public static class ScopeExtensions
	{
		public static IScope getRoot(this IScope scope)
		{
			while (scope.Parent_ != null)
				scope = scope.Parent_;

			return scope;
		}

		/// Create Replica invariant path.

		public static string makePath(this IScope scope)
		{
			var sb = new StringBuilder();
			addPath(scope, sb);
			return sb.ToString();
		}

		/// Create replica invariant path (note that '/' are not allowed in names).

		public static string makePath(this IScope scope, string name)
		{
			return scope.enter(name).makePath();
		}

		static void addPath(IScope scope, StringBuilder sb)
		{
			if (scope.Parent_ != null)
			{
				addPath(scope.Parent_, sb);
				if (sb.Length != 0)
					sb.Append("/");
			}

			sb.Append(scope.Name);
		}

	}
}