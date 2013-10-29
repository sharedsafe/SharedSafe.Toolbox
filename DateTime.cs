using System;
using System.Diagnostics;

namespace Toolbox
{
	public interface IDateTime
	{
		/// The original date / time (kind field is Unspecified, it is neither local, nor Utc)
		DateTime DateTime { get; }

		/// The offset the original DateTime differs from Utc.
		TimeSpan Offset { get; }
	}

	public sealed class ZonedDateTime : IDateTime
	{
		public ZonedDateTime(DateTime dateTime, TimeSpan offset)
		{
			DateTime = dateTime;
			Offset = offset;
		}

		#region IDateTime Members

		public DateTime DateTime { get; private set; }
		public TimeSpan Offset { get; private set; }

		#endregion
	}

	public static class DateTimeExtensions
	{
		public static IDateTime toIDateTime(this DateTime dt)
		{
			/// note: GetUtcOffset already does the magic if dt is Utc
			return new ZonedDateTime(dt, TimeZone.CurrentTimeZone.GetUtcOffset(dt));
		}

		/// The time in it was at that time in Greenwich == Original - Offset)

		public static DateTime toUTC(this IDateTime dateTime)
		{
			DateTime dt = dateTime.DateTime - dateTime.Offset;
			Debug.Assert(dt.Kind == DateTimeKind.Unspecified);
			return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
		}

		/// The time, in the timezone we are right now.

		public static DateTime toLocal(this IDateTime dateTime)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime(dateTime.toUTC());
		}
	}

	public static class TimeSpanExtensions
	{
		// http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time

		public static string toHumanReadable(this TimeSpan ts)
		{
			var delta = ts.TotalSeconds;

			const int Second = 1;
			const int Minute = 60 * Second;
			const int Hour = 60 * Minute;
			const int Day = 24 * Hour;
			const int Month = 30 * Day;

			if (delta < 0)
				return "not yet";
			if (delta < 1 * Minute)
				return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
			if (delta < 2 * Minute)
				return "a minute ago";
			if (delta < 45 * Minute)
				return ts.Minutes + " minutes ago";
			if (delta < 90 * Minute)
				return "an hour ago";
			if (delta < 24 * Hour)
				return ts.Hours + " hours ago";
			if (delta < 48 * Hour)
				return "yesterday";
			if (delta < 30 * Day)
				return ts.Days + " days ago";
			if (delta < 12 * Month)
			{
				int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return months <= 1 ? "one month ago" : months + " months ago";
			}
			int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
			return years <= 1 ? "one year ago" : years + " years ago";
		}
	}

}
