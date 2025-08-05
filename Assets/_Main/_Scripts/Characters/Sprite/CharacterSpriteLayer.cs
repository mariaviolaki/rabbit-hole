using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";

		readonly SpriteCharacter spriteCharacter;
		readonly UITransitionHandler transitionHandler;
		readonly SpriteLayerType layerType;
		bool isSwappingContainers = false;

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

		Coroutine skippedSpriteCoroutine;
		Coroutine skippedDirectionCoroutine;
		Coroutine skippedBrightnessCoroutine;
		Coroutine skippedColorCoroutine;

		public SpriteLayerType LayerType => layerType;
		public string SpriteName => spriteCharacter.GetRawSpriteName(primaryImage.sprite.name);

		public bool IsChangingDirection => directionCoroutine != null || skippedDirectionCoroutine != null;
		public bool IsChangingBrightness => brightnessCoroutine != null || skippedBrightnessCoroutine != null;
		public bool IsChangingColor => colorCoroutine != null || skippedColorCoroutine != null;

		public CharacterSpriteLayer(SpriteCharacter spriteCharacter, UITransitionHandler transitionHandler, SpriteLayerType layerType, Transform root, bool isFacingRight)
		{
			this.spriteCharacter = spriteCharacter;
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

		public Coroutine SetSprite(Sprite sprite, bool isImmediate, float speed = 0)
		{
			if (skippedSpriteCoroutine != null) return null;
			bool isSkipped = spriteCharacter.Manager.StopProcess(ref spriteCoroutine);
			RestoreContainerAfterSkip(isSkipped);

			if (isImmediate)
			{
				SetSpriteImmediate(sprite);
				return null;
			}
			else if (isSkipped)
			{
				skippedSpriteCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeSprite(sprite, speed, isSkipped));
				return skippedSpriteCoroutine;
			}
			else
			{
				spriteCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeSprite(sprite, speed, isSkipped));
				return spriteCoroutine;
			}
		}

		public Coroutine FaceRight(bool isImmediate, float speed = 0) => Flip(isImmediate, speed);
		public Coroutine FaceLeft(bool isImmediate, float speed = 0) => Flip(isImmediate, speed);
		Coroutine Flip(bool isImmediate, float speed = 0)
		{
			if (skippedDirectionCoroutine != null) return null;
			bool isSkipped = spriteCharacter.Manager.StopProcess(ref directionCoroutine);
			RestoreContainerAfterSkip(isSkipped);

			if (isImmediate)
			{
				SetDirectionImmediate();
				return null;
			}
			else if (isSkipped)
			{
				skippedDirectionCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeDirection(speed, isSkipped));
				return skippedDirectionCoroutine;
			}
			else
			{
				directionCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeDirection(speed, isSkipped));
				return directionCoroutine;
			}
		}

		public Coroutine SetBrightness(Color color, bool isImmediate, float speed = 0)
		{
			if (skippedBrightnessCoroutine != null) return null;
			bool isSkipped = spriteCharacter.Manager.StopProcess(ref brightnessCoroutine);

			if (isImmediate)
			{
				SetBrightnessImmediate(color);
				return null;
			}
			else if (isSkipped)
			{
				skippedBrightnessCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeBrightness(color, speed, isSkipped));
				return skippedBrightnessCoroutine;
			}
			else
			{
				brightnessCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeBrightness(color, speed, isSkipped));
				return brightnessCoroutine;
			}
		}

		public Coroutine SetColor(Color color, bool isImmediate, float speed = 0)
		{
			if (skippedColorCoroutine != null) return null;
			bool isSkipped = spriteCharacter.Manager.StopProcess(ref colorCoroutine);

			if (isImmediate)
			{
				SetColorImmediate(color);
				return null;
			}
			else if (isSkipped)
			{
				skippedColorCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeColor(color, speed, isSkipped));
				return skippedColorCoroutine;
			}
			else
			{
				colorCoroutine = spriteCharacter.Manager.StartCoroutine(ChangeColor(color, speed, isSkipped));
				return colorCoroutine;
			}
		}

		void SetSpriteImmediate(Sprite sprite)
		{
			primaryImage.sprite = sprite;
			secondaryImage.sprite = sprite;
		}

		void SetDirectionImmediate()
		{
			Vector3 currentLocalScale = primaryImage.transform.localScale;
			Vector3 targetLocalScale = new(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);
			primaryImage.transform.localScale = targetLocalScale;
			secondaryImage.transform.localScale = targetLocalScale;
		}

		void SetBrightnessImmediate(Color color)
		{
			primaryImage.color = color;
			secondaryImage.color = color;
		}

		void SetColorImmediate(Color color)
		{
			primaryImage.color = color;
			secondaryImage.color = color;
		}

		IEnumerator ChangeSprite(Sprite sprite, float speed, bool isSkipped)
		{
			while (isSwappingContainers) yield return null;
			isSwappingContainers = true;

			ToggleSecondaryImage(true);

			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.sprite = sprite;
			yield return transitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, speed);

			ToggleSecondaryImage(false);

			isSwappingContainers = false;
			if (isSkipped) skippedSpriteCoroutine = null;
			else spriteCoroutine = null;
		}

		IEnumerator ChangeDirection(float speed, bool isSkipped)
		{
			float defaultSpeed = spriteCharacter.Manager.GameOptions.Characters.TransitionSpeed;
			speed = spriteCharacter.GetTransitionSpeed(speed, defaultSpeed, isSkipped);

			Vector3 currentLocalScale = primaryImage.transform.localScale;
			Vector3 targetLocalScale = new(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);

			while (isSwappingContainers) yield return null;
			isSwappingContainers = true;

			ToggleSecondaryImage(true);

			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.transform.localScale = targetLocalScale;
			yield return transitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, speed);

			ToggleSecondaryImage(false);

			isSwappingContainers = false;
			if (isSkipped) skippedDirectionCoroutine = null;
			else directionCoroutine = null;
		}

		IEnumerator ChangeBrightness(Color color, float speed, bool isSkipped)
		{
			yield return transitionHandler.SetColor(primaryImage, color, speed);

			if (isSkipped) skippedBrightnessCoroutine = null;
			else brightnessCoroutine = null;
		}

		IEnumerator ChangeColor(Color color, float speed, bool isSkipped)
		{
			yield return transitionHandler.SetColor(primaryImage, color, speed);

			if (isSkipped) skippedColorCoroutine = null;
			else colorCoroutine = null;
		}

		void RestoreContainerAfterSkip(bool isSkipped)
		{
			if (!isSkipped || primaryImageContainer.activeSelf) return;

			ToggleSecondaryImage(false);
			isSwappingContainers = false;
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
