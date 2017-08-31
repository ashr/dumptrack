using System;
using System.Collections.Generic;

namespace dumptrack
{
	public interface IDumpTrackModule
	{
		List<Offence> FindOffences ();
	}
}

