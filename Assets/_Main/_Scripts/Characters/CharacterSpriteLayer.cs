using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		const float SpriteTransitionMultiplier = 0.5f;
		const float ColorTransitionMultiplier = 0.5f;

		CharacterManager characterManager;
		Image image;
		CanvasGroup oldCanvasGroup;

		Coroutine spriteCoroutine;
		Coroutine colorCoroutine;

		public SpriteLayerType LayerType { get; private set; }
		public bool IsChangingColor { get { return colorCoroutine != null; } }

		public CharacterSpriteLayer(SpriteLayerType layerType, Image image, CharacterManager characterManager)
		{
			this.characterManager = characterManager;
			this.image = image;
			LayerType = layerType;
		}

		public Coroutine SetColor(Color color, float speed)
		{
			if (image.color == color) return null;

			characterManager.StopProcess(ref colorCoroutine);

			speed = speed <= 0 ? characterManager.GameOptions.ColorTransitionSpeed : speed;
			spriteCoroutine = characterManager.StartCoroutine(TransitionColor(color, speed));
			return spriteCoroutine;
		}

		public Coroutine SetSprite(Sprite sprite, float speed)
		{
			if (image.sprite.name == sprite.name) return null;

			characterManager.StopProcess(ref spriteCoroutine);
			ReplaceOldImage(sprite);

			speed = speed <= 0 ? characterManager.GameOptions.SpriteTransitionSpeed : speed;
			spriteCoroutine = characterManager.StartCoroutine(TransitionSprite(speed));
			return spriteCoroutine;
		}

		IEnumerator TransitionColor(Color color, float speed)
		{
			Color startColor = image.color;
			float progress = 0f;
			float colorDistance = Vector4.Distance(startColor, color);

			while (progress < 1f)
			{
				// Use the same speed for short and long distances
				progress += (speed * ColorTransitionMultiplier * Time.deltaTime) / colorDistance;
				progress = Mathf.Clamp01(progress);

				// Ease in & out
				float smoothProgress = Mathf.SmoothStep(0, 1, progress);
				image.color = Color.Lerp(startColor, color, smoothProgress);

				yield return null;
			}

			// Ensure exact final color
			image.color = color;
			colorCoroutine = null;
		}

		IEnumerator TransitionSprite(float speed)
		{
			CanvasGroup newCanvasGroup = image.GetComponent<CanvasGroup>();

			while (newCanvasGroup.alpha < 1 || (oldCanvasGroup != null && oldCanvasGroup.alpha > 0))
			{
				float fadeSpeed = speed * SpriteTransitionMultiplier * Time.deltaTime;

				newCanvasGroup.alpha = Mathf.MoveTowards(newCanvasGroup.alpha, 1, fadeSpeed);

				if (oldCanvasGroup != null)
				{
					oldCanvasGroup.alpha = Mathf.MoveTowards(oldCanvasGroup.alpha, 0, fadeSpeed);
					if (oldCanvasGroup.alpha <= 0)
						Object.Destroy(oldCanvasGroup.gameObject);
				}

				yield return null;
			}

			spriteCoroutine = null;
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
