namespace Dialogue
{
	public static class TreeNodeUtilities
	{
		public static string GetDialogueNodeId(string sceneName, int nodeId)
		{
			if (sceneName == null || nodeId < 0) return null;

			return $"{sceneName}:{nodeId}";
		}
	}
}
