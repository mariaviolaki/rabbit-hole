using System.Collections.Generic;

namespace History
{
	[System.Serializable]
	public class PlayerProgress
	{
		public List<string> readLines;

		// Default constructor in case this class couldn't be loaded from a file
		public PlayerProgress()
		{
			readLines = new List<string>();
		}
	}
}
