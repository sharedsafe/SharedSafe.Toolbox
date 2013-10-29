using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

#if DEBUG && !TOOLBOX_ESSENTIALS
using NUnit.Framework;
#endif


namespace Toolbox.IO
{
	public static class Paths
	{
		#region Path

		public static string appendPath(this string path, string another)
		{
			return Path.Combine(path, another);
		}

		public static string parentPath(this string path)
		{
			return Path.GetDirectoryName(path);
		}

		public static string getPathsName(this string path)
		{
			return Path.GetFileName(path);
		}

		/// Returns the paths extension including the leading ".".

		public static string getPathExtension(this string path)
		{
			return Path.GetExtension(path);
		}

		public static string stripPathExtension(this string path)
		{
			var ext = Path.GetExtension(path);
			return path.Substring(0, path.Length - ext.Length);
		}

		public static string getSignificantPathsName(this string path)
		{
			var name = path.getPathsName();
			return name == string.Empty ? path : name;
		}

		#endregion

		/**
			Tries to converts an arbitrary string to a filename representation.
		**/

		public static string suggestFilename(this string name, string fallback)
		{
			var mb = name.suggestFilename();
			return mb.HasValue ? mb.Value : fallback;
		}

		public static Maybe<string> suggestFilename(this string name)
		{
			if (name.Length == 0)
				return Maybe.Nothing;

			/// We simply replace all non conforming characters to a ' ' character.

			var r = new char[name.Length];

			for (int i = 0; i != name.Length; ++i)
			{
				char c = name[i];
				if (-1 == Array.IndexOf(InvalidFileNameChars, c))
					r[i] = c;
				else
					r[i] = ' ';
			}

			return new string(r).toMaybe();
		}

		static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

		public static StrictPath toStrictPath(this string path)
		{
			return new StrictPath(path);
		}

		public static StrictPath toStrictPath(this IEnumerable<string> components)
		{
			return new StrictPath(components);
		}

		public static LenientPath toLenientPath(this string path)
		{
			return new LenientPath(path);
		}

		public static IEnumerable<StrictPath> toStrictPaths(this IEnumerable<string> paths)
		{
			return from p in paths select p.toStrictPath();
		}

		public static IEnumerable<LenientPath> toLenientPaths(this IEnumerable<string> paths)
		{
			return from p in paths select p.toLenientPath();
		}
	}

	// for some reason we are not able to refer nunit on pcslave

#if DEBUG && !TOOLBOX_ESSENTIALS

	[TestFixture]
	public class PathsTest
	{
		[Test]
		public void testPathIdentifier()
		{
			Assert.That("C:/test\\xxx".toLenientPath(), Is.EqualTo("c:/test/xxx".toLenientPath()));
		}

		[Test]
		public void testParentPathOf()
		{
			Assert.That("C:\\".toLenientPath().isParentOf("c:/xx".toLenientPath()), Is.EqualTo(true));
			Assert.That("C:\\".toLenientPath().isParentOf("c:".toLenientPath()), Is.EqualTo(false));
			Assert.That("xxxx/yy/z".toLenientPath().isParentOf("xxxx\\Yy/z/z".toLenientPath()), Is.EqualTo(true));
			Assert.That("".toLenientPath().isParentOf("".toLenientPath()), Is.EqualTo(false));
			Assert.That("xx".toLenientPath().isParentOf("".toLenientPath()), Is.EqualTo(false));
			Assert.That("".toLenientPath().isParentOf("ff".toLenientPath()), Is.EqualTo(false));
		}

		[Test]
		public void testSignificantPathsName()
		{
			// interesting, returns string.Empty here
			Assert.That("C:\\".getSignificantPathsName(), Is.EqualTo("C:\\"));
			Assert.That("C:".getSignificantPathsName(), Is.EqualTo("C:"));
			Assert.That("C:hello".getSignificantPathsName(), Is.EqualTo("hello"));
			Assert.That("C:\\hello".getSignificantPathsName(), Is.EqualTo("hello"));
			Assert.That("C:hello\\test".getSignificantPathsName(), Is.EqualTo("test"));
			Assert.That("C:\\hello\\test".getSignificantPathsName(), Is.EqualTo("test"));
			// this one could be unexpected, but fine for now
			Assert.That("C:\\hello\\test\\".getSignificantPathsName(), Is.EqualTo("C:\\hello\\test\\"));
		}
	}

#endif
}
