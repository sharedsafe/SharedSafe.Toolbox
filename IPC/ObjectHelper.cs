
using System.Security.Principal;

namespace Toolbox.IPC
{
	static class ObjectHelper
	{
		public static string makePipeName(string name, bool global)
		{
			string prefix = string.Empty;

			if (!global)
			{
				var identity = WindowsIdentity.GetCurrent();
				if (identity == null)
					throw new InternalError("No User SID");

				prefix = identity.User + ".";
			}

			return prefix + "JsonObjectPipe.{0}".format(name);
		}
	}
}
