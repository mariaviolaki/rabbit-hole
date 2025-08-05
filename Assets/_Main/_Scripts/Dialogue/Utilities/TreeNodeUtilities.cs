namespace Dialogue
{
	public static class TreeNodeUtilities
	{
		public static string GetDialogueNodeId(string sectionName, int nodeId)
		{
			if (sectionName == null || nodeId < 0) return null;

			return $"{sectionName}:{nodeId}";
		}
	}
}
