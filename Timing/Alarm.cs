
using System.Diagnostics;

namespace Toolbox.Timing
{
	public sealed class Alarm
	{
		public readonly uint InMilliseconds;
		long? _endTick;

		public Alarm(uint inMilliseconds)
		{
			InMilliseconds = inMilliseconds;
		}

		public void start()
		{
			requireStopped();
			_endTick = Ticks.Current + Ticks.fromMilliseconds(InMilliseconds.signed());
		}

		public void stop()
		{
			requireActive();
			_endTick = null;
		}

		public bool IsActive
		{
			get { return _endTick != null; }
		}

		public void end()
		{
			requireActive();
			_endTick = Ticks.Current;
		}

		public void reset()
		{
			stop();
			start();
		}

		public bool IsRinging
		{
			get
			{
				return TimeLeftMS == 0;
			}
		}

		public uint TimeLeftMS
		{
			get
			{
				requireActive();
				var current = Ticks.Current;
				var ticksLeft = _endTick - current;
				return ticksLeft <= 0 ? 0 : Ticks.toMilliseconds(ticksLeft.Value).unsigned();
			}
		}

		void requireActive()
		{
			Debug.Assert(IsActive);
		}

		void requireStopped()
		{
			Debug.Assert(!IsActive);
		}
	}
}
