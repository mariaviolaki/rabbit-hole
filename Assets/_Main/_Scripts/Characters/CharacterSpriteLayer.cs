using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		CharacterManager characterManager;
		Coroutine brightnessCoroutine;
		Coroutine colorCoroutine;

		public SpriteLayerType LayerType { get; private set; }
		public Image LayerImage { get; private set; }
		public CanvasGroup LayerCanvasGroup { get; private set; }
		public bool IsChangingBrightness => brightnessCoroutine != null;
		public bool IsChangingColor => colorCoroutine != null;

		public CharacterSpriteLayer(SpriteLayerType layerType, Image image, CharacterManager characterManager)
		{
			this.characterManager = characterManager;
			LayerType = layerType;
			LayerImage = image;
			LayerCanvasGroup = image.GetComponent<CanvasGroup>();
		}

		public void SetBrightnessCoroutine(Coroutine brightnessCoroutine)
		{
			characterManager.StopProcess(ref this.brightnessCoroutine);
			this.brightnessCoroutine = brightnessCoroutine;
		}

		public void SetColorCoroutine(Coroutine colorCoroutine)
		{
			characterManager.StopProcess(ref this.colorCoroutine);
			this.colorCoroutine = colorCoroutine;
		}

		public void StopBrightnessCoroutine()
		{
			characterManager.StopProcess(ref brightnessCoroutine);
		}

		public void StopColorCoroutine()
		{
			characterManager.StopProcess(ref colorCoroutine);
		}
	}
}
