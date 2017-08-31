using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using HtmlAgilityPack;

namespace dumptrack
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			DumpTrack dumpTrack = new DumpTrack ();
			dumpTrack.FindDumps ();
			dumpTrack.Notify ();

			return;
		}
	}
}
