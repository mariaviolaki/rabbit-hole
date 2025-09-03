using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTreeMap
	{
		public List<string> SceneNames = new();
		public List<string> TreeNames = new();
		public List<string> SceneTitles = new();
		[System.NonSerialized] Dictionary<string, string> SceneLookup;
		[System.NonSerialized] Dictionary<string, string> SceneTitleLookup;

		public string GetTreeName(string sceneName) =>
			sceneName != null && SceneLookup.TryGetValue(sceneName, out string treeName) ? treeName : null;
		public string GetSceneTitle(string sceneName) =>
			sceneName != null && SceneTitleLookup.TryGetValue(sceneName, out string sceneTitle) ? sceneTitle : null;

		public void Initialize()
		{
			SceneLookup = new();
			SceneTitleLookup = new();

			for (int i = 0; i < SceneNames.Count; i++)
			{
				string sceneName = SceneNames[i];
				SceneLookup[sceneName] = TreeNames[i];
				SceneTitleLookup[sceneName] = SceneTitles[i];
			}
		}
	}
}
