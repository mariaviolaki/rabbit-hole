using System.Collections.Generic;
using Visuals;

namespace History
{
	[System.Serializable]
	public class PlayerProgress
	{
		public string playerName;
		public int saveMenuPage;
		public List<string> readLines;
		public List<UnlockedCG> unlockedCGs;

		// Default constructor in case this class couldn't be loaded from a file
		public PlayerProgress()
		{
			saveMenuPage = 1;
			readLines = new List<string>();
			unlockedCGs = new List<UnlockedCG>();
		}
	}
}
