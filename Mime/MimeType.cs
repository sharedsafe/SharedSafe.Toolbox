using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Toolbox.Mime
{
	public struct MimeType
	{
		public MimeType(string type, string subtype)
		{
			Type = type;
			Subtype = subtype;
		}

		public string Type;
		public string Subtype;

		public override string ToString()
		{
			return Type + "/" + Subtype;
		}

		#region Static

		public static MimeType? fromPath(string path)
		{
			return fromExtension(Path.GetExtension(path));
		}

		/// Extension is expected to include the .
		
		public static MimeType? fromExtension(string extension)
		{
			MimeType r;
			return MimeTypes.TryGetValue(extension.ToLowerInvariant(), out r) ? r : (MimeType?) null;
		}

		#endregion

		static readonly Dictionary<string, MimeType> MimeTypes = makeMimeTypes();

		/// This reads the mime types from the registry

		static Dictionary<string, MimeType> makeMimeTypes()
		{
			var dict = new Dictionary<string, MimeType>();

			try
			{
				using (var regKey = Registry.ClassesRoot.OpenSubKey("Mime\\Database\\Content Type"))
				{
					if (regKey == null)
						return dict;

					var mimeTypes = regKey.GetSubKeyNames();
					foreach (var mimeType in mimeTypes)
					{
						var parts = mimeType.Split('/');
						if (parts.Length != 2)
							continue;

						using (var key = regKey.OpenSubKey(mimeType))
						{
							if (key == null)
								continue;

							var ext = key.GetValue("Extension") as string;
							if (ext != null)
								dict[ext.ToLowerInvariant()] = new MimeType(parts[0], parts[1]);
						}
					}
				}
			}
			// ReSharper disable EmptyGeneralCatchClause
			catch
			// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return dict;
		}

		// todo: add standard mime types here

		public static class Application
		{
			public static readonly MimeType OctetStream = new MimeType("application", "octet-stream");
		}
	}
}
