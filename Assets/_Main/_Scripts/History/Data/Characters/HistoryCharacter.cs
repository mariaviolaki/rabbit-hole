using Characters;
using System.Collections.Generic;
using UnityEngine;
using Variables;

namespace History
{
	[System.Serializable]
	public class HistoryCharacter
	{
		// Character
		public int priority;
		public bool isVisible;
		public Vector2 position;
		public Color color;
		public bool isHighlighted;
		public Vector2 direction;
		public List<HistoryAnimationData> animations = new();

		// Character Data
		public CharacterType type;
		public string shortName;
		public string name;
		public string displayName;

		// Sprite Character Data
		public List<HistorySpriteLayer> spriteLayers = new();

		// 3D Model Character Data
		public string modelExpression;
	}

	[System.Serializable]
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

	[System.Serializable]
	public class HistoryAnimationData
	{
		public DataTypeEnum type;
		public string name;
		public string value;
	}
}
