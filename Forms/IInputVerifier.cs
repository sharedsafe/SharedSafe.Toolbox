using System;

namespace Toolbox.Forms
{
	public interface IInputVerifier
	{
		void register(Func<bool> verify, Action highlight, Action focus);
		bool verify();
		void highlightAndFocusFirstUnverified();
	}

}
