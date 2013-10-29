using System;
using System.Diagnostics;
using System.IO;

namespace Toolbox.IPC
{
	static class ProcessHelper
	{
		public static string replaceInvalidFileNameCharacters(string path)
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();
			const char ReplacementChar = '_';
			Debug.Assert(-1 == Array.IndexOf(invalidChars, ReplacementChar));

			var target = new char[path.Length];

			for (var i = 0; i != path.Length; ++i)
			{
				char c = path[i];
				if (-1 != Array.IndexOf(invalidChars, c))
					c = ReplacementChar;

				target[i] = c;
			}

			return new string(target);
		}
	}
}
