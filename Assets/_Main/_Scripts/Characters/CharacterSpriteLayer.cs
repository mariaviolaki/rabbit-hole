using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		public SpriteLayerType LayerType { get; private set; }
		public Image LayerImage { get; private set; }
		public CanvasGroup LayerCanvasGroup { get; private set; }

		public Coroutine ColorCoroutine { get; set; }
		public Coroutine BrightnessCoroutine { get; set; }

		public CharacterSpriteLayer(SpriteLayerType layerType, Image image)
		{
			LayerType = layerType;
			LayerImage = image;
			LayerCanvasGroup = image.GetComponent<CanvasGroup>();
		}
	}
}
