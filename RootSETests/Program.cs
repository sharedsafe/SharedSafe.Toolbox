using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RootSE.Provider;

namespace RootSETests
{
	static class Program
	{
		public static void Main(string[] args)
		{
			var basics = new Basics();
			basics.storeAndRetrieve();
		}
	}
}
