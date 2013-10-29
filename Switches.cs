using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Toolbox
{
	public sealed class ExclusiveSwitch : IDisposable
	{
		readonly Action _on;
		readonly Action _off;
		bool _active;

		public ExclusiveSwitch(Func<IDisposable> onOff)
		{
			IDisposable v = null;

			_on = () => v = onOff();
			_off = () => v.Dispose();
		}

		public ExclusiveSwitch(Action on, Action off)
		{
			_on = on;
			_off = off;
		}

		public void Dispose()
		{
			switchOff();
		}

		public void switchOn()
		{
			if (_active)
				return;

			_on();
			_active = true;
		}

		public void switchOff()
		{
			if (!_active)
				return;

			_off();
			_active = false;
		}
	}

	public sealed class ScopedSwitch
	{
		readonly Action _on;
		readonly Action _off;

		static readonly Action EmptyAction = () => { };

		public ScopedSwitch()
			: this (EmptyAction, EmptyAction)
		{
		}

		public ScopedSwitch(Action on, Action off)
		{
			_on = on;
			_off = off;
		}

		int _count;

		public IDisposable begin()
		{
			++_count;

			if (_count == 1)
				_on();

			return new DisposeAction(end);
		}

		void end()
		{
			Debug.Assert(_count != 0);
			--_count;

			if (_count == 0)
				_off();
		}

		public bool Value { get { return _count != 0; } }
	}


#if DEBUG

	[TestFixture]
	public sealed class ScopedBooleanTests
	{
		[Test]
		public void test()
		{
			var b = new ScopedSwitch();

			using (b.begin())
			{
				Assert.True(b.Value);
			}

			Assert.False(b.Value);
		}
	}
#endif
}
