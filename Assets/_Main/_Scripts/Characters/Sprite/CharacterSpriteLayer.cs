using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";

		readonly CharacterManager characterManager;
		readonly UITransitionHandler transitionHandler;
		readonly SpriteLayerType layerType;

		GameObject primaryImageContainer;
		Image primaryImage;
		CanvasGroup primaryCanvasGroup;

		GameObject secondaryImageContainer;
		Image secondaryImage;
		CanvasGroup secondaryCanvasGroup;
		
		Coroutine spriteCoroutine;
		Coroutine directionCoroutine;
		Coroutine brightnessCoroutine;
		Coroutine colorCoroutine;

		public SpriteLayerType LayerType => layerType;
		public bool IsChangingDirection => directionCoroutine != null;
		public bool IsChangingSprite => spriteCoroutine != null;
		public bool IsChangingBrightness => brightnessCoroutine != null;
		public bool IsChangingColor => colorCoroutine != null;

		public CharacterSpriteLayer(CharacterManager characterManager, UITransitionHandler transitionHandler, SpriteLayerType layerType, Transform root)
		{
			this.characterManager = characterManager;
			this.transitionHandler = transitionHandler;
			this.layerType = layerType;

			primaryImageContainer = root.GetChild(0).gameObject;
			primaryImageContainer.name = primaryContainerName;
			primaryImage = primaryImageContainer.GetComponent<Image>();
			primaryCanvasGroup = primaryImageContainer.GetComponent<CanvasGroup>();

			// Create a secondary image for smooth transitions
			secondaryImageContainer = Object.Instantiate(primaryImageContainer, root);
			secondaryImageContainer.name = secondaryContainerName;
			secondaryImage = secondaryImageContainer.GetComponent<Image>();
			secondaryCanvasGroup = secondaryImageContainer.GetComponent<CanvasGroup>();

			ToggleSecondaryImage(false);
		}

		public void SetSpriteInstant(Sprite sprite)
		{
			characterManager.StopProcess(ref spriteCoroutine);
			primaryImage.sprite = sprite;
		}
		public Coroutine SetSprite(Sprite sprite, float speed)
		{
			characterManager.StopProcess(ref spriteCoroutine);
			spriteCoroutine = characterManager.StartCoroutine(ChangeSprite(sprite, speed));
			return spriteCoroutine;
		}

		public void FlipInstant()
		{
			characterManager.StopProcess(ref directionCoroutine);
			Vector3 currentLocalScale = primaryImage.transform.localScale;
			primaryImage.transform.localScale = new Vector3(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);
		}
		public Coroutine Flip(float speed)
		{
			characterManager.StopProcess(ref directionCoroutine);
			directionCoroutine = characterManager.StartCoroutine(ChangeDirection(speed));
			return directionCoroutine;
		}

		public void SetBrightnessInstant(Color color)
		{
			characterManager.StopProcess(ref brightnessCoroutine);
			primaryImage.color = color;
		}
		public Coroutine SetBrightness(Color color, float speed)
		{
			characterManager.StopProcess(ref brightnessCoroutine);
			brightnessCoroutine = characterManager.StartCoroutine(ChangeBrightness(color, speed));
			return brightnessCoroutine;
		}

		public void SetColorInstant(Color color)
		{
			characterManager.StopProcess(ref colorCoroutine);
			primaryImage.color = color;
		}
		public Coroutine SetColor(Color color, float speed)
		{
			characterManager.StopProcess(ref colorCoroutine);
			colorCoroutine = characterManager.StartCoroutine(ChangeColor(color, speed));
			return colorCoroutine;
		}

		IEnumerator ChangeSprite(Sprite sprite, float speed)
		{
			ToggleSecondaryImage(true);

			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.sprite = sprite;

			yield return transitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, speed);

			ToggleSecondaryImage(false);

			spriteCoroutine = null;
		}

		IEnumerator ChangeDirection(float speed)
		{
			ToggleSecondaryImage(true);

			Vector3 currentLocalScale = primaryImage.transform.localScale;
			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.transform.localScale = new Vector3(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);

			yield return transitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, speed);

			ToggleSecondaryImage(false);
			directionCoroutine = null;
		}

		IEnumerator ChangeBrightness(Color color, float speed)
		{
			yield return transitionHandler.SetColor(primaryImage, color, speed);
			brightnessCoroutine = null;
		}

		IEnumerator ChangeColor(Color color, float speed)
		{
			yield return transitionHandler.SetColor(primaryImage, color, speed);
			colorCoroutine = null;
		}

		void ToggleSecondaryImage(bool isActive)
		{
			if (!isActive)
			{
				// Swap the visual containers because the graphic is now in the second one
				SwapContainers();
				secondaryCanvasGroup.alpha = primaryCanvasGroup.alpha;
				secondaryImage.transform.localScale = secondaryImage.transform.localScale;
				secondaryImage.color = primaryImage.color;
				secondaryImage.sprite = primaryImage.sprite;
			}

			secondaryImageContainer.SetActive(isActive);
		}

		void SwapContainers()
		{
			GameObject tempImageContainer = primaryImageContainer;
			Image tempImage = primaryImage;
			CanvasGroup tempCanvasGroup = primaryCanvasGroup;

			primaryImageContainer = secondaryImageContainer;
			primaryImage = secondaryImage;
			primaryCanvasGroup = secondaryCanvasGroup;

			secondaryImageContainer = tempImageContainer;
			secondaryImage = tempImage;
			secondaryCanvasGroup = tempCanvasGroup;
		}
	}
}