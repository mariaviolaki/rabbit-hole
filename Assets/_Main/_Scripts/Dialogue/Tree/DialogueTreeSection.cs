using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTreeSection
	{
		public string Name;
		public int MinId;
		public int MaxId;
		public List<DialogueTreeNode> Nodes;

		public DialogueTreeSection(string name)
		{
			MinId = -1;
			MaxId = -1;
			Name = name;
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
