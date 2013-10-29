using System.Reflection;
using Toolbox.Serialization;
using SIO = System.IO;

namespace Toolbox.Sync
{
	[Obfuscation]
	public abstract class Attributes
	{
		public ulong LastModificationTime { get; set; }
	}

	#region File Specific

	// todo: this should not be here :(

	[Obfuscation]
	public abstract class ItemAttributes : Attributes
	{
		public SIO.FileAttributes Flags { get; set; }
	}

	[Obfuscation]
	[TypeAlias("Sync.FolderAttributes")]
	public sealed class FolderAttributes : ItemAttributes
	{
	};

	[Obfuscation]
	[TypeAlias("Sync.FileAttributes")]
	public sealed class FileAttributes : ItemAttributes
	{
		public ulong Length { get; set; }
		// optional!!!!! may always be null!
		public HashBlocks Hash { get; set; }
	}

	[Obfuscation]
	public enum HashFormat
	{
		SHA1 = 0
	}

	[Obfuscation]
	public sealed class HashBlocks
	{
		// The hash  format that is being used.
		public HashFormat Format { get; set; }
		// Number of bytes each block covers
		public uint BlockSize { get; set; }
		// Base64 Encoded hash bytes.
		public string[] Blocks { get; set; }
	}

	#endregion
}
