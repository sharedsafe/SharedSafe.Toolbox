using System;
using System.Collections.Generic;
using System.Linq;
using P = System.IO.Path;

namespace Toolbox.IO
{
	/**
		A strict path is a comparable, case sensitive path.

		The path's is 
		- trimmed
		- it's separator is always the system's separator (alternative's are converted to the system's separator)
		- no trailing separator is allowed and is stripped.
		- casing is _not_ converted.
		- ToString() returns the strict path.
	**/

	public struct StrictPath : IComparable<StrictPath>
	{
		public readonly string Path;

		internal StrictPath(string path)
		{
			path = path.Trim();
			if (System.IO.Path.DirectorySeparatorChar != System.IO.Path.AltDirectorySeparatorChar)
				path = path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

			Path = path.TrimEnd(DirectorySeparatorCharInArray);
		}

		internal StrictPath(IEnumerable<string> components)
		{
			Path = string.Join(DirectorySeparatorString, (from c in components select c.Trim()).ToArray());
		}

		public bool isParentOf(StrictPath child)
		{
			return child.Path.StartsWith(Path + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal);
		}

		public StrictPath makeChildRelative(StrictPath child)
		{
			if (this == child)
				return Empty;

			if (!isParentOf(child))
				throw new Exception("Path is not same or parent of child, can not make child relative.");

			var toStrip = Path.Length + 1;

			return new StrictPath(child.Path.Substring(toStrip, child.Path.Length - toStrip));
		}

		public string[] Components
		{
			get
			{
				return Path.Split(DirectorySeparatorCharInArray, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		#region Combinators

		public StrictPath combinedWith(StrictPath other)
		{
			return new StrictPath(P.Combine(Path, other.Path));
		}

		public StrictPath parentOf()
		{
			return new StrictPath(P.GetDirectoryName(Path));
		}

		#endregion

		#region IComparable<StrictPath> Members

		public int CompareTo(StrictPath other)
		{
			return Path.CompareTo(other.Path);
		}

		#endregion

		#region R# EQuality

		public bool Equals(StrictPath other)
		{
			return Equals(other.Path, Path);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (StrictPath))
				return false;
			return Equals((StrictPath) obj);
		}

		public override int GetHashCode()
		{
			return (Path != null ? Path.GetHashCode() : 0);
		}

		public static bool operator ==(StrictPath left, StrictPath right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(StrictPath left, StrictPath right)
		{
			return !left.Equals(right);
		}

		#endregion

		public override string ToString()
		{
			return Path;
		}

		internal static readonly char[] DirectorySeparatorCharInArray = new[]
		{
			System.IO.Path.DirectorySeparatorChar
		};

		static readonly string DirectorySeparatorString = new string(System.IO.Path.DirectorySeparatorChar, 1);
		public static readonly StrictPath Empty = new StrictPath("");
	}
}