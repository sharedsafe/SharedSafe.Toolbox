using System;

namespace Toolbox.Sync.Detail
{
	public sealed class RelocationError : Exception
	{
		public RelocationError(string description, Pair<IItem, string>[] items, Exception exception_)
			: base(makeDetailedDescription(description, exception_), exception_)
		{
			Description = description;
			Items = items;
		}

		static string makeDetailedDescription(string description, Exception e_)
		{
			if (e_ == null)
				return description;

			return description + ": " + e_.Message;
		}

		public readonly string Description;
		public readonly Pair<IItem, string>[] Items;
	}
}
