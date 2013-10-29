using System;

namespace Toolbox.FileSync
{
	public static class DateTimeConverter
	{
		public static ulong toExternal(this DateTime dt)
		{
			long ticks = dt.Ticks;
			if (ticks < 0)
				throw new InternalError("Got invalid ticks from DateTime", dt);

			return (ulong)ticks;
		}

		public static DateTime toUtcDate(this ulong extValue)
		{
			var ticks = (long)extValue;
			if (ticks < 0)
				throw new InternalError("Got invalid ticks from external value", extValue);

			return new DateTime(ticks, DateTimeKind.Utc);
		}
	}
}
