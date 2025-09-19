using Gameplay;
using IO;

namespace Visuals
{
	[System.Serializable]
	public class CharacterCG
	{
		public CharacterRoute route;
		public int num; // CG number (1-based)
		public int stage; // stage index (0-based)
		public int stageCount;

		public string ImageName => $"{route}{FilePaths.CGSeparator}{num}{FilePaths.CGSeparator}{stage}";

		public CharacterCG(CharacterRoute route, int num, int stage)
		{
			this.route = route;
			this.num = num;
			this.stage = stage;
			stageCount = 1;
		}		
	}
}
