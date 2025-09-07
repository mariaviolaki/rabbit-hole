using System.Collections.Generic;

namespace History
{
	[System.Serializable]
	public class PlayerProgress
	{
		public string playerName;
		public int saveMenuPage;
		public List<string> readLines;

		// Default constructor in case this class couldn't be loaded from a file
		public PlayerProgress()
		{
			saveMenuPage = 1;
			readLines = new List<string>();
		}
	}
}
