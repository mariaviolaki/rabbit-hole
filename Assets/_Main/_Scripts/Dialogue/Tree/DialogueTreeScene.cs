using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTreeScene
	{
		public string Name;
		public string Title;
		public int MinId;
		public int MaxId;
		public List<DialogueTreeNode> Nodes;

		public DialogueTreeScene(string name, string title)
		{
			MinId = -1;
			MaxId = -1;
			Name = name;
			Title = title ?? name;
			Nodes = new List<DialogueTreeNode>();
		}

		public List<DialogueTreeNode> GetNodes()
		{
			List<DialogueTreeNode> allNodes = new();
			foreach (DialogueTreeNode node in Nodes)
			{
				allNodes.AddRange(node.GetNodes());
			}
			return allNodes;
		}
	}
}
