namespace Dialogue
{
	public static class TreeNodeUtilities
	{
		const char NodeIdSeparator = ':';

		public static string GetDialogueNodeId(string sceneName, int nodeNum)
		{
			if (string.IsNullOrWhiteSpace(sceneName) || nodeNum < 0) return null;

			return $"{sceneName}{NodeIdSeparator}{nodeNum}";
		}

		public static string GetSceneFromNodeId(string nodeId)
		{
			string[] idParts = ParseNodeId(nodeId);
			if (idParts == null) return null;

			return idParts[0];
		}

		public static int GetNumFromNodeId(string nodeId)
		{
			string[] idParts = ParseNodeId(nodeId);
			if (idParts == null) return -1;

			if (int.TryParse(idParts[1], out int num))
				return num;
			else
				return -1;
		}

		static string[] ParseNodeId(string nodeId)
		{
			if (string.IsNullOrWhiteSpace(nodeId)) return null;

			string[] idParts = nodeId.Split(NodeIdSeparator);
			if (idParts.Length != 2) return null;

			return idParts;
		}
	}
}
