using Characters;
using System.Collections.Generic;
using UnityEngine;
using Variables;

namespace History
{
	[System.Serializable]
	public abstract class HistoryCharacterBase
	{
		// Character
		public int priority;
		public bool isVisible;
		public Vector2 position;
		public Color color;
		public bool isHighlighted;
		public bool isFacingRight;
		public List<HistoryAnimationData> animations;

		// Character Data
		public CharacterType type;
		public string shortName;
		public string name;
		public string displayName;
	}

	[System.Serializable]
	public class HistorySpriteCharacter : HistoryCharacterBase
	{
		public class HistorySpriteLayer
		{
			public SpriteLayerType layerType;
			public string spriteName;

			public HistorySpriteLayer(SpriteLayerType layerType, string spriteName)
			{
				this.layerType = layerType;
				this.spriteName = spriteName;
			}
		}

		public List<HistorySpriteLayer> spriteLayers = new();
	}

	[System.Serializable]
	public class HistoryModel3DCharacter : HistoryCharacterBase
	{
		public string expression;
	}

	[System.Serializable]
	public class HistoryAnimationData
	{
		public DataTypeEnum type;
		public string name;
		public string value;
	}
}
