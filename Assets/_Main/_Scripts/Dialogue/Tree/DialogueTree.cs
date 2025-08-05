using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTree
	{
		public string Name;
		public List<DialogueTreeSection> Sections;

		[System.NonSerialized] Dictionary<string, DialogueTreeSection> sectionLookup;
		[System.NonSerialized] List<DialogueTreeNode> nodeLookup;

		public DialogueTreeSection GetSection(string name) => sectionLookup.TryGetValue(name, out var section) ? section : null;
		public DialogueTreeNode GetNode(int id) => id >= 0 && id < nodeLookup.Count ? nodeLookup[id] : null;

		public void Initialize()
		{
			sectionLookup = new();
			nodeLookup = new();

			foreach (DialogueTreeSection section in Sections)
			{
				sectionLookup[section.Name] = section;
				nodeLookup.AddRange(section.GetNodes());
			}
		}
	}
}
