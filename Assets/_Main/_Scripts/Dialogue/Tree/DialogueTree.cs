using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTree
	{
		public string Name;
		public List<DialogueTreeScene> Scenes;

		[System.NonSerialized] Dictionary<string, DialogueTreeScene> sceneLookup;
		[System.NonSerialized] List<DialogueTreeNode> nodeLookup;

		public DialogueTreeScene GetScene(string name) => sceneLookup.TryGetValue(name, out var scene) ? scene : null;
		public DialogueTreeNode GetNode(int id) => id >= 0 && id < nodeLookup.Count ? nodeLookup[id] : null;

		public void Initialize()
		{
			sceneLookup = new();
			nodeLookup = new();

			foreach (DialogueTreeScene scene in Scenes)
			{
				sceneLookup[scene.Name] = scene;
				nodeLookup.AddRange(scene.GetNodes());
			}
		}
	}
}
