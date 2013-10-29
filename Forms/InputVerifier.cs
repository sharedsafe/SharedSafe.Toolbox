using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Forms
{
	public sealed class InputVerifier : IInputVerifier
	{
		readonly List<VerifyableInput> _inputs = new List<VerifyableInput>();

		public void register(Func<bool> verify, Action highlight, Action focus)
		{
			register(new VerifyableInputImpl(verify, highlight, focus));
		}

		internal void register(VerifyableInput input)
		{
			_inputs.Add(input);
		}

		public bool verify()
		{
			return _inputs.All(input => input.verify());
		}

		public void highlightAndFocusFirstUnverified()
		{
			var first_ = (from input in _inputs where !input.verify() select input).FirstOrDefault();
			if (first_ != null)
			{
				first_.highlight();
				first_.focus();
			}
		}

		sealed class VerifyableInputImpl : VerifyableInput
		{
			readonly Func<bool> _verify;
			readonly Action _highlight;
			readonly Action _focus;

			public VerifyableInputImpl(Func<bool> verify, Action highlight, Action focus)
			{
				_verify = verify;
				_highlight = highlight;
				_focus = focus;
			}

			#region VerifyableInput Members

			public bool verify()
			{
				return _verify();
			}

			public void highlight()
			{
				_highlight();
			}

			public void focus()
			{
				_focus();
			}

			#endregion
		}

	}
}
