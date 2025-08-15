using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class CharacterSpriteLayer : MonoBehaviour
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";
		const float SpriteSpeedMultiplier = 0.5f;

		SpriteCharacter spriteCharacter;
		SpriteLayerType layerType;

		GameObject primaryImageContainer;
		Image primaryImage;
		CanvasGroup primaryCanvasGroup;

		GameObject secondaryImageContainer;
		Image secondaryImage;
		CanvasGroup secondaryCanvasGroup;

		Sprite sprite;
		float spriteSpeed;
		TransitionStatus spriteStatus = TransitionStatus.Completed;

		public SpriteLayerType LayerType => layerType;
		public string SpriteName => spriteCharacter.GetRawSpriteName(sprite.name);
		public TransitionStatus SpriteStatus => spriteStatus;
		public Color DisplayColor => primaryImage.color;
		public Sprite LayerSprite { get { return sprite; } set { sprite = value; } }

		void Update()
		{
			TransitionSprite();
		}

		public void Initialize(SpriteCharacter spriteCharacter, SpriteLayerType layerType, Transform root, Sprite defaultSprite)
		{
			this.spriteCharacter = spriteCharacter;
			this.layerType = layerType;

			primaryImageContainer = root.GetChild(0).gameObject;
			primaryImageContainer.name = primaryContainerName;
			primaryImage = primaryImageContainer.GetComponent<Image>();
			primaryCanvasGroup = primaryImageContainer.GetComponent<CanvasGroup>();

			// Create a secondary image for smooth transitions
			secondaryImageContainer = Instantiate(primaryImageContainer, root);
			secondaryImageContainer.name = secondaryContainerName;
			secondaryImage = secondaryImageContainer.GetComponent<Image>();
			secondaryCanvasGroup = secondaryImageContainer.GetComponent<CanvasGroup>();

			primaryCanvasGroup.alpha = 1f;
			secondaryCanvasGroup.alpha = 0f;
			primaryImageContainer.SetActive(true);
			secondaryImageContainer.SetActive(false);

			sprite = defaultSprite;
			SetSpriteImmediate();
		}

		public void SetSprite(Sprite sprite, bool isImmediate = false, float speed = 0)
		{
			if (sprite == this.sprite && spriteStatus == TransitionStatus.Completed) return;

			this.sprite = sprite;

			if (isImmediate)
			{
				SetSpriteImmediate();
				spriteStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = spriteCharacter.GameOptions.Characters.TransitionSpeed;
				bool isSkipped = spriteStatus != TransitionStatus.Completed;
				spriteSpeed = spriteCharacter.GetTransitionSpeed(speed, defaultSpeed, SpriteSpeedMultiplier, isSkipped);
				spriteStatus = spriteStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void SkipSpriteTransition()
		{
			if (spriteStatus == TransitionStatus.Completed) return;

			float defaultSpeed = spriteCharacter.GameOptions.General.TransitionSpeed;
			spriteSpeed = spriteCharacter.GetTransitionSpeed(defaultSpeed, defaultSpeed, SpriteSpeedMultiplier, true);
			spriteStatus = TransitionStatus.Skipped;
		}

		void SetSpriteImmediate()
		{
			primaryImage.sprite = sprite;
			secondaryImage.sprite = sprite;
		}

		public void SetColorImmediate(Color color)
		{
			primaryImage.color = color;
			secondaryImage.color = color;
		}

		void TransitionSprite()
		{
			if (spriteStatus == TransitionStatus.Completed) return;

			if (!secondaryImageContainer.activeInHierarchy)
				secondaryImageContainer.SetActive(true);

			float speed = spriteSpeed * Time.deltaTime;
			secondaryImage.sprite = sprite;
			primaryCanvasGroup.alpha = Mathf.MoveTowards(primaryCanvasGroup.alpha, 0f, speed);
			secondaryCanvasGroup.alpha = Mathf.MoveTowards(secondaryCanvasGroup.alpha, 1f, speed);

			if (Utilities.AreApproximatelyEqual(secondaryCanvasGroup.alpha, 1f))
			{
				primaryImage.sprite = sprite;
				primaryCanvasGroup.alpha = 1f;
				secondaryCanvasGroup.alpha = 0f;
				secondaryImageContainer.SetActive(false);
				spriteStatus = TransitionStatus.Completed;
			}
		}

		public void TransitionColor(Color color, float speed)
		{
			float transitionSpeed = speed * Time.deltaTime;
			primaryImage.color = Vector4.MoveTowards(primaryImage.color, color, transitionSpeed);
			secondaryImage.color = Vector4.MoveTowards(secondaryImage.color, color, transitionSpeed);
		}
	}
}
