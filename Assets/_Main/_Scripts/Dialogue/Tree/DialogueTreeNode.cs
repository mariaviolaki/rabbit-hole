using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTreeNode
	{
		public DialogueNodeType Type;
		public int Id;
		public int MaxId;
		public int NextId;
		public List<string> Data;
		public List<DialogueTreeNode> Children;

		[System.NonSerialized] public DialogueTreeNode Parent;

		public DialogueTreeNode(DialogueNodeType type, DialogueTreeNode parent)
		{
			Type = type;
			NextId = -1;
			Data = new List<string>();
			Children = new List<DialogueTreeNode>();
			Parent = parent;
		}

		public void SetId(int id)
		{
			Id = id;
			MaxId = id;
		}

		public List<DialogueTreeNode> GetNodes()
		{
			List<DialogueTreeNode> allNodes = new() { this };
			foreach (DialogueTreeNode child in Children)
			{
				allNodes.AddRange(child.GetNodes());
			}
			return allNodes;
		}
	}
}
