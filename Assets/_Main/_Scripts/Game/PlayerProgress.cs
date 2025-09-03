using Dialogue;
using Gameplay;
using System.Collections.Generic;

namespace History
{
	[System.Serializable]
	public class PlayerProgress
	{
		public string playerName;
		public CharacterRoute route;
		public string sceneTitle;
		public int saveMenuPage;
		public List<string> readLines;

		// Default constructor in case this class couldn't be loaded from a file
		public PlayerProgress()
		{
			route = CharacterRoute.Common;
			sceneTitle = DialogueFlowController.StartSceneName;
			saveMenuPage = 1;
			readLines = new List<string>();
		}
	}
}
