using System;

namespace Toolbox.Forms
{
	interface VerifyableInput
	{
		bool verify();
		void highlight();
		void focus();
	}
}
