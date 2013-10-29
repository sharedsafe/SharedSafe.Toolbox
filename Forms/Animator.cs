using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Toolbox.Meta;

namespace Toolbox.Forms
{
	sealed class Animator
	{
		[ThreadStatic]
		internal static readonly Animator ThreadLocalInstance = new Animator();

		readonly Timer Timer = new Timer();
		uint CurrentTime;

		const int RefreshIntervalMS = 10;

		readonly Dictionary<Animation, uint> _animations = new Dictionary<Animation, uint>();

		internal Animator()
		{
			Timer.Tick += (s, args) => update();
		}

		internal void add(Animation animation)
		{
			kickTimer();
			_animations.Add(animation, CurrentTime);
		}

		void kickTimer()
		{
			if (Timer.Enabled)
			{
				Debug.Assert(_animations.Count != 0);
				return;
			}
			Debug.Assert(_animations.Count == 0);
			CurrentTime = 0;
			Timer.Interval = RefreshIntervalMS;
			Timer.Start();
			Debug.Assert(Timer.Enabled);
		}

		void update()
		{
			Debug.Assert(Timer.Enabled);
			CurrentTime += RefreshIntervalMS;
			
			var removeList = new List<Animation>();

			foreach (var animation in _animations)
			{
				var anim = animation.Key;
				var offset = animation.Value;
				Debug.Assert(offset < CurrentTime);

				var current = CurrentTime - offset;
				if (current >= anim.Duration)
				{
					anim.end();
					removeList.Add(anim);
					continue;
				}

				double v = (double)current / anim.Duration;
				anim.update(v);
			}

			foreach (var anim in removeList)
				_animations.Remove(anim);

			if (_animations.Count == 0)
				Timer.Enabled = false;
		}
	};


	abstract class Animation
	{
		// true on finished.
		public readonly uint Duration;
		public abstract void update(double f);
		public abstract void end();

		protected Animation(uint duration)
		{
			Duration = duration;
		}
	};

	sealed class Animation<ControlT, ValueT> : Animation
		where ControlT : Control
	{
		Control _control_;
		readonly MemberAccessor<ValueT> _memberAccessor;
		readonly ValueT _start;
		readonly ValueT _final;
		readonly Func<ValueT, ValueT, double, ValueT> _mixer;

		public Animation(
			ControlT control, 
			MemberAccessor<ValueT> memberAccessor, 
			ValueT final, 
			uint duration,
			Func<ValueT, ValueT, double, ValueT> mixer)
			: base(duration)
		{
			_control_ = control;
			_memberAccessor = memberAccessor;
			_start = memberAccessor.Get();
			_final = final;
			_mixer = mixer;

			control.Disposed += freeze;
		}

		public override void update(double f)
		{
			if (_control_ != null)
				_memberAccessor.Set(_mixer(_start, _final, f));
		}


		public override void end()
		{
			if (_control_ == null)
				return;

			_memberAccessor.Set(_final);
			detachFromControl();
		}

		void freeze(object sender, EventArgs args)
		{
			detachFromControl();
		}

		void detachFromControl()
		{
			_control_.Disposed -= freeze;
			_control_ = null;
		}
	}
}
