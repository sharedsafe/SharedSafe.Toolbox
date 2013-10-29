using System.Diagnostics;

namespace Toolbox.Timing
{
	public static class Ticks
	{
		public static long Current { get { return Stopwatch.GetTimestamp(); } }

		#region Milliseconds

		// We support negative milliseconds to handle duration computations symmetrically.

		public static long fromMilliseconds(uint milliseconds)
		{
			return (long) (MS2Ticks*milliseconds);
		}

		public static long fromMilliseconds(int milliseconds)
		{
			return (long)(MS2Ticks * milliseconds);
		}

		public static int toMilliseconds(long ticks)
		{
			return (int)(Ticks2MS * ticks);
		}

		#endregion

		#region Microseconds

		public static long fromMicroseconds(long microseconds)
		{
			return (long) (Microseconds2Ticks*microseconds);
		}

		public static long toMicroseconds(long ticks)
		{
			return (long) (Ticks2Microseconds*ticks);
		}

		#endregion

		#region 100 Nanosecond units

		public static long to100NS(long ticks)
		{
			return (long) (ticks*Ticks2100NS);
		}

		public static long from100NS(long ns)
		{
			return (long) (ns*HNS2Ticks);
		}

		#endregion

		static readonly double TicksPerSecond = Stopwatch.Frequency;

		static readonly double Ticks2100NS = 10*1000*1000/TicksPerSecond;
		static readonly double Ticks2Microseconds = 1000*1000/TicksPerSecond;
		static readonly double HNS2Ticks = 1.0/Ticks2100NS;
		
		static readonly double Ticks2MS = 1000 / TicksPerSecond;

		static readonly double MS2Ticks = 1.0/Ticks2MS;
		static readonly double Microseconds2Ticks = 1.0 / Ticks2Microseconds;
	}
}
