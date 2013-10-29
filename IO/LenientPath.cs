using System;

namespace Toolbox.IO
{
	/**
		Similar to a StrictPath, but uses a lowercase instance of the path for comparison.
		ToString() return the lenient path.
	**/

	public struct LenientPath : IComparable<LenientPath>
	{
		readonly string Path;
		readonly string Lenient;

		internal LenientPath(string path)
		{
			path = path.Trim();
			if (System.IO.Path.DirectorySeparatorChar != PathIdentifierDirectorySeparator)
				path = path.Replace(System.IO.Path.DirectorySeparatorChar, PathIdentifierDirectorySeparator);

			Path = path.TrimEnd(PathIdentifierSeparatorInArray);
			Lenient = Path.ToLowerInvariant();
		}

		public bool isParentOf(LenientPath child)
		{
			return child.Lenient.StartsWith(Lenient + PathIdentifierDirectorySeparator, StringComparison.Ordinal);
		}

		public LenientPath makeChildRelative(LenientPath child)
		{
			if (!isParentOf(child))
				throw new Exception("Path is not parent of child, can not make child relative.");

			var toStrip = Path.Length + 1;

			return new LenientPath(child.Path.Substring(toStrip, child.Path.Length - toStrip));
		}

		public string[] StrictComponents
		{
			get
			{
				return Path.Split(PathIdentifierSeparatorInArray, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		#region IComparable<LenientPath> Members

		public int CompareTo(LenientPath other)
		{
			return Lenient.CompareTo(other.Lenient);
		}

		#endregion

		#region R# EQuality (note: uses Lenient instead of Path)

		public bool Equals(LenientPath other)
		{
			return Equals(other.Lenient, Lenient);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof(LenientPath))
				return false;
			return Equals((LenientPath)obj);
		}

		public override int GetHashCode()
		{
			return (Lenient != null ? Lenient.GetHashCode() : 0);
		}

		public static bool operator ==(LenientPath left, LenientPath right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LenientPath left, LenientPath right)
		{
			return !left.Equals(right);
		}

		#endregion

		public override string ToString()
		{
			return Lenient;
		}

		internal const char PathIdentifierDirectorySeparator = '/';
		internal static readonly char[] PathIdentifierSeparatorInArray = new[]
		{
			PathIdentifierDirectorySeparator
		};
	}
}
