using System.Collections;
using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		const float MoveSpeedMultiplier = 100f;

		Coroutine moveCoroutine;
		protected Coroutine directionCoroutine;
		protected Coroutine visibilityCoroutine;
		protected Coroutine colorCoroutine;
		protected Coroutine brightnessCoroutine;

		protected CanvasGroup rootCanvasGroup;
		protected Animator animator;
		protected bool isFacingRight;
		protected bool isHighlighted = true;

		private Vector2 currentPos = Vector2.zero;

		protected Color DisplayColor { get { return isHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; set; } = Color.white;
		protected Color DarkColor { get { return GetDarkColor(LightColor); } }

		public abstract void FlipInstant();
		public abstract void ChangeBrightnessInstant(bool isHighlighted);
		public abstract void ChangeColorInstant(Color color);
		protected abstract IEnumerator FlipDirection(float speed, bool isSkipped);
		protected abstract IEnumerator ChangeBrightness(bool isHighlighted, float speed, bool isSkipped);
		protected abstract IEnumerator ChangeColor(Color color, float speed, bool isSkipped);

		protected override IEnumerator Init()
		{
			// Load this character's prefab into the scene
			yield return manager.FileManager.LoadCharacterPrefab(data.CastName);

			GameObject prefab = manager.FileManager.GetCharacterPrefab(data.CastName);
			if (prefab == null) yield break;

			GameObject rootGameObject = Object.Instantiate(prefab, manager.Container);
			root = rootGameObject.GetComponent<RectTransform>();
			rootCanvasGroup = root.GetComponent<CanvasGroup>();
			animator = root.GetComponentInChildren<Animator>();

			root.name = data.Name;
			rootCanvasGroup.alpha = 0f;
			isFacingRight = manager.GameOptions.Characters.AreSpritesFacingRight;
		}

		public void SetPriority(int index)
		{
			if (!isVisible) return;

			manager.SetPriority(data.ShortName, index);
		}

		public void ShowInstant() => ChangeVisibilityInstant(true);
		public Coroutine Show(float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(true, speed, isSkipped));
			return visibilityCoroutine;
		}
		
		public void HideInstant() => ChangeVisibilityInstant(false);
		public Coroutine Hide(float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(false, speed, isSkipped));
			return visibilityCoroutine;
		}

		public void SetPositionXInstant(float x) => SetPositionInstant(new Vector2(x, currentPos.y));
		public void SetPositionYInstant(float y) => SetPositionInstant(new Vector2(currentPos.x, y));
		public void SetPositionInstant(Vector2 normalizedPos)
		{
			manager.StopProcess(ref moveCoroutine);

			currentPos = normalizedPos;
			root.anchoredPosition = GetTargetPosition(normalizedPos);
		}

		public Coroutine SetPositionX(float x, float speed = 0) => SetPosition(new Vector2(x, currentPos.y), speed);
		public Coroutine SetPositionY(float y, float speed = 0) => SetPosition(new Vector2(currentPos.x, y), speed);
		public Coroutine SetPosition(Vector2 normalizedPos, float speed = 0)
		{
			currentPos = normalizedPos;

			bool isSkipped = manager.StopProcess(ref moveCoroutine);

			moveCoroutine = manager.StartCoroutine(MoveCharacter(normalizedPos, speed, isSkipped));
			return moveCoroutine;
		}

		public void FaceLeftInstant()
		{
			if (!isFacingRight) return;
			FlipInstant();
		}
		public Coroutine FaceLeft(float speed = 0)
		{
			if (!isFacingRight) return null;
			return Flip(speed);
		}

		public void FaceRightInstant()
		{
			if (isFacingRight) return;
			FlipInstant();
		}
		public Coroutine FaceRight(float speed = 0)
		{
			if (isFacingRight) return null;
			return Flip(speed);
		}

		public Coroutine Flip(float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref directionCoroutine);

			directionCoroutine = manager.StartCoroutine(FlipDirection(speed, isSkipped));
			return directionCoroutine;
		}

		public void HighlightInstant() => ChangeBrightnessInstant(true);
		public Coroutine Highlight(float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(true, speed, isSkipped));
			return brightnessCoroutine;
		}

		public void UnhighlightInstant() => ChangeBrightnessInstant(false);
		public Coroutine Unhighlight(float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(false, speed, isSkipped));
			return brightnessCoroutine;
		}

		public void SetColorInstant(Color color) => ChangeColorInstant(color);
		public Coroutine SetColor(Color color, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref colorCoroutine);

			colorCoroutine = manager.StartCoroutine(ChangeColor(color, speed, isSkipped));
			return colorCoroutine;
		}

		public void SetAnimation(string animationName)
		{
			animator.SetTrigger(animationName);
		}

		public void SetAnimation(string animationName, bool isPlaying)
		{
			animator.SetBool(animationName, isPlaying);
		}

		protected float GetTransitionSpeed(float speedInput, float defaultSpeed, bool isTransitionSkipped)
		{
			if (isTransitionSkipped)
				return manager.GameOptions.General.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return defaultSpeed;
			else
				return speedInput;
		}

		protected Color GetDarkColor(Color lightColor)
		{
			return new Color(
				lightColor.r * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.g * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.b * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.a
			);
		}

		void ChangeVisibilityInstant(bool isVisible)
		{
			manager.StopProcess(ref visibilityCoroutine);

			rootCanvasGroup.alpha = isVisible ? 1f : 0f;
			this.isVisible = isVisible;
		}
		IEnumerator ChangeVisibility(bool isVisible, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.FadeTransitionSpeed, isSkipped);

			yield return Utils.TransitionUtils.SetImageVisibility(rootCanvasGroup, isVisible, speed);

			this.isVisible = isVisible;
			visibilityCoroutine = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.MoveSpeed, isSkipped);

			Vector2 startPos = root.anchoredPosition;
			Vector2 endPos = GetTargetPosition(normalizedPos);
			float distance = Vector2.Distance(startPos, endPos);

			float distancePercent = 0f;
			while (distancePercent < 1f)
			{
				// Move at the same speed regardless of distance
				distancePercent += (speed * MoveSpeedMultiplier * Time.deltaTime) / distance;
				distancePercent = Mathf.Clamp01(distancePercent);
				// Move smoothly towards the start and end
				float smoothDistance = Mathf.SmoothStep(0f, 1f, distancePercent);
				root.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothDistance);

				yield return null;
			}

			root.anchoredPosition = endPos;
			moveCoroutine = null;
		}

		Vector2 GetTargetPosition(Vector2 normalizedPos)
		{
			Vector2 containerSize = manager.Container.rect.size;
			Vector2 rootSize = root.rect.size;

			// Calculate target position in parent space
			Vector2 targetPos = normalizedPos * containerSize;

			// Clamp the target pos so the sprite stays inside
			Vector2 pivotOffset = root.pivot * rootSize;
			Vector2 minPos = pivotOffset;
			Vector2 maxPos = containerSize - rootSize + pivotOffset;
			Vector2 clampedTargetPos = new Vector2(
				Mathf.Clamp(targetPos.x, minPos.x, maxPos.x),
				Mathf.Clamp(targetPos.y, minPos.y, maxPos.y)
			);

			// Convert anchor to offset from bottom-left of container
			Vector2 anchorCenter = (root.anchorMin + root.anchorMax) * 0.5f;
			Vector2 anchorOffset = Vector2.Scale(anchorCenter, containerSize);

			// Return position relative to anchor center
			return clampedTargetPos - anchorOffset;
		}
	}
}
