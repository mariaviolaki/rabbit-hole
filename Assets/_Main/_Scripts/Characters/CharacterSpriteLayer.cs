using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		const float FadeSpeedMultiplier = 0.5f;

		CharacterManager characterManager;
		Image image;
		CanvasGroup oldCanvasGroup;

		Coroutine fadeProcess;

		public SpriteLayerType LayerType { get; private set; }

		public CharacterSpriteLayer(SpriteLayerType layerType, Image image, CharacterManager characterManager)
		{
			this.characterManager = characterManager;
			this.image = image;
			LayerType = layerType;
		}

		public void SetSprite(Sprite sprite)
		{
			image.sprite = sprite;
		}

		public Coroutine TransitionSprite(Sprite sprite, float speed)
		{
			if (image.sprite.name == sprite.name) return null;
			else if (fadeProcess != null) return fadeProcess;

			ReplaceOldImage(sprite);

			speed = speed <= 0 ? characterManager.GameOptions.SpriteTransitionSpeed : speed;
			fadeProcess = characterManager.StartCoroutine(FadeSprites(speed));
			return fadeProcess;
		}

		IEnumerator FadeSprites(float speed)
		{
			CanvasGroup newCanvasGroup = image.GetComponent<CanvasGroup>();

			while (newCanvasGroup.alpha < 1 || (oldCanvasGroup != null && oldCanvasGroup.alpha > 0))
			{
				float fadeSpeed = speed * FadeSpeedMultiplier * Time.deltaTime;

				newCanvasGroup.alpha = Mathf.MoveTowards(newCanvasGroup.alpha, 1, fadeSpeed);

				if (oldCanvasGroup != null)
				{
					oldCanvasGroup.alpha = Mathf.MoveTowards(oldCanvasGroup.alpha, 0, fadeSpeed);
					if (oldCanvasGroup.alpha <= 0)
						Object.Destroy(oldCanvasGroup.gameObject);
				}

				yield return null;
			}

			fadeProcess = null;
		}

		void ReplaceOldImage(Sprite newSprite)
		{
			if (oldCanvasGroup != null)
				Object.Destroy(oldCanvasGroup.gameObject);

			// Set the old image to the current image (we only need to change its opacity now)
			oldCanvasGroup = image.GetComponent<CanvasGroup>();

			// Create a copy of the old image and cache the new one
			image = Object.Instantiate(image, image.transform.parent);
			image.name = "Image";
			image.sprite = newSprite;
			image.GetComponent<CanvasGroup>().alpha = 0;
		}
	}
}
