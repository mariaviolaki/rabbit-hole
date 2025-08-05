using System.Collections.Generic;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTreeMap
	{
		public List<string> SectionNames = new();
		public List<string> TreeNames = new();
		[System.NonSerialized] Dictionary<string, string> SectionLookup;

		public string GetTreeName(string sectionName) => SectionLookup.TryGetValue(sectionName, out string treeName) ? treeName : null;

		public void Initialize()
		{
			SectionLookup = new();

			for (int i = 0; i < SectionNames.Count; i++)
			{
				string sectionName = SectionNames[i];
				SectionLookup[sectionName] = TreeNames[i];
			}
		}
	}
}
