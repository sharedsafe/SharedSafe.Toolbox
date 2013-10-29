using System;

namespace Zip2SecureSplitter
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				internalMain(args);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: " + e.Message);
				return 1;
			}
		}

		static void internalMain(string[] args)
		{
			if (args.Length != 2)
				throw new Exception("Usage Zip2SecureSplitter.exe [split|join] BaseFileName.exe");

			switch (args[0])
			{
				case "split":
					Splitter.split(args[1]);
					break;

				case "join":
					Splitter.join(args[1]);
					break;

				default:
					throw new Exception("Expecting 'split' or 'join' in the first argument");
			}
		}
	}
}
