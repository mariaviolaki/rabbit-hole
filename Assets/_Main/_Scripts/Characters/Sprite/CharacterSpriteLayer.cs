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
		bool isFacingRight;

		public SpriteLayerType LayerType => layerType;
		public bool IsChangingDirection => directionCoroutine != null;
		public bool IsChangingSprite => spriteCoroutine != null;
		public bool IsChangingBrightness => brightnessCoroutine != null;
		public bool IsChangingColor => colorCoroutine != null;

		public CharacterSpriteLayer(CharacterManager characterManager, UITransitionHandler transitionHandler, SpriteLayerType layerType, Transform root, bool isFacingRight)
		{
			this.characterManager = characterManager;
			this.transitionHandler = transitionHandler;
			this.layerType = layerType;
			this.isFacingRight = isFacingRight;

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

		public Coroutine SetSprite(Sprite sprite, bool isImmediate, float speed = 0)
		{
			characterManager.StopProcess(ref spriteCoroutine);

			if (isImmediate)
			{
				primaryImage.sprite = sprite;
				return null;
			}
			else
			{
				spriteCoroutine = characterManager.StartCoroutine(ChangeSprite(sprite, speed));
				return spriteCoroutine;
			}
		}

		public Coroutine FaceRight(bool isImmediate, float speed = 0)
		{
			if (isFacingRight) return null;
			return Flip(isImmediate, speed);
		}

		public Coroutine FaceLeft(bool isImmediate, float speed = 0)
		{
			if (!isFacingRight) return null;
			return Flip(isImmediate, speed);
		}

		Coroutine Flip(bool isImmediate, float speed = 0)
		{
			characterManager.StopProcess(ref directionCoroutine);

			if (isImmediate)
			{
				Vector3 currentLocalScale = primaryImage.transform.localScale;
				primaryImage.transform.localScale = new Vector3(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);
				isFacingRight = !isFacingRight;
				return null;
			}
			else
			{
				directionCoroutine = characterManager.StartCoroutine(ChangeDirection(speed));
				return directionCoroutine;
			}
		}

		public Coroutine SetBrightness(Color color, bool isImmediate, float speed = 0)
		{
			characterManager.StopProcess(ref brightnessCoroutine);

			if (isImmediate)
			{
				primaryImage.color = color;
				return null;
			}
			else
			{
				brightnessCoroutine = characterManager.StartCoroutine(ChangeBrightness(color, speed));
				return brightnessCoroutine;
			}
		}

		public Coroutine SetColor(Color color, bool isImmediate, float speed = 0)
		{
			characterManager.StopProcess(ref colorCoroutine);

			if (isImmediate)
			{
				primaryImage.color = color;
				return null;
			}
			else
			{
				colorCoroutine = characterManager.StartCoroutine(ChangeColor(color, speed));
				return colorCoroutine;
			}
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
			isFacingRight = !isFacingRight;

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
				secondaryImage.transform.localScale = primaryImage.transform.localScale;
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