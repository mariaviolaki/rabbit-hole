using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		const float MoveSpeedMultiplier = 100f;
		const float FadeSpeedMultiplier = 5f;
		const float ColorSpeedMultiplier = 5f;

		Coroutine moveCoroutine;
		protected Coroutine directionCoroutine;
		protected Coroutine visibilityCoroutine;
		protected Coroutine colorCoroutine;
		protected Coroutine brightnessCoroutine;

		protected CanvasGroup canvasGroup;
		protected Animator animator;
		protected bool isFacingRight;
		protected bool isHighlighted = true;

		protected Color DisplayColor { get { return isHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; set; } = Color.white;
		protected Color DarkColor { get { return GetDarkColor(LightColor); } }

		public abstract void FlipInstant();
		public abstract void ChangeBrightnessInstant(bool isHighlighted);
		public abstract void ChangeColorInstant(Color color);
		protected abstract IEnumerator FlipDirection(float speed);
		protected abstract IEnumerator ChangeBrightness(bool isHighlighted, float speed);
		protected abstract IEnumerator ChangeColor(Color color, float speed);

		protected async override Task Init()
		{
			// Load this character's prefab into the scene
			GameObject prefab = await manager.FileManager.LoadCharacterPrefab(data.CastName);
			GameObject rootGameObject = Object.Instantiate(prefab, manager.Container);

			root = rootGameObject.GetComponent<RectTransform>();
			canvasGroup = root.GetComponent<CanvasGroup>();
			animator = root.GetComponentInChildren<Animator>();

			root.name = data.Name;
			canvasGroup.alpha = 0f;
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
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(true, speed));
			return visibilityCoroutine;
		}
		
		public void HideInstant() => ChangeVisibilityInstant(false);
		public Coroutine Hide(float speed = 0)
		{
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(false, speed));
			return visibilityCoroutine;
		}

		public void SetPositionXInstant(float x) => SetPositionInstant(new Vector2(x, root.anchoredPosition.y));
		public void SetPositionYInstant(float y) => SetPositionInstant(new Vector2(root.anchoredPosition.x, y));
		public void SetPositionInstant(Vector2 normalizedPos) => root.anchoredPosition = GetTargetPosition(normalizedPos);
		public Coroutine SetPositionX(float x, float speed = 0) => SetPosition(new Vector2(x, root.anchoredPosition.y), speed);
		public Coroutine SetPositionY(float y, float speed = 0) => SetPosition(new Vector2(root.anchoredPosition.x, y), speed);
		public Coroutine SetPosition(Vector2 normalizedPos, float speed = 0)
		{
			manager.StopProcess(ref moveCoroutine);

			moveCoroutine = manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
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
			manager.StopProcess(ref directionCoroutine);

			directionCoroutine = manager.StartCoroutine(FlipDirection(speed));
			return directionCoroutine;
		}

		public void HighlightInstant() => ChangeBrightnessInstant(true);
		public Coroutine Highlight(float speed = 0)
		{
			manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(true, speed));
			return brightnessCoroutine;
		}

		public void UnhighlightInstant() => ChangeBrightnessInstant(false);
		public Coroutine Unhighlight(float speed = 0)
		{
			manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(false, speed));
			return brightnessCoroutine;
		}

		public void SetColorInstant(Color color) => ChangeColorInstant(color);
		public Coroutine SetColor(Color color, float speed = 0)
		{
			manager.StopProcess(ref colorCoroutine);

			colorCoroutine = manager.StartCoroutine(ChangeColor(color, speed));
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

		protected virtual IEnumerator FadeImage(CanvasGroup canvasGroup, bool isFadeIn, float speed)
		{
			float startAlpha = canvasGroup.alpha;
			float targetAlpha = isFadeIn ? 1f : 0f;
			speed = speed <= 0f ? manager.GameOptions.Characters.FadeTransitionSpeed : speed;

			float timeElapsed = 0;
			float duration = (1f / speed) * FadeSpeedMultiplier * Mathf.Abs(targetAlpha - startAlpha);
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothPercentage);
				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
		}

		protected virtual IEnumerator SetImageBrightness(Graphic image, bool isHighlighted, float speed)
		{
			Color startColor = image.color;
			Color targetColor = isHighlighted ? LightColor : DarkColor;

			speed = speed <= 0 ? manager.GameOptions.Characters.BrightnessTransitionSpeed : speed;
			float duration = (1 / speed) * ColorSpeedMultiplier * Vector4.Distance(startColor, targetColor);

			float timeElapsed = 0;
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				image.color = Color.Lerp(startColor, targetColor, smoothPercentage);
				yield return null;
			}

			image.color = targetColor;
		}

		protected virtual IEnumerator ColorImage(Graphic image, Color color, float speed)
		{
			Color startColor = image.color;
			Color targetColor = isHighlighted ? color : GetDarkColor(color);

			speed = speed <= 0 ? manager.GameOptions.Characters.ColorTransitionSpeed : speed;
			float duration = (1 / speed) * ColorSpeedMultiplier * Vector4.Distance(startColor, targetColor);

			float timeElapsed = 0;
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				image.color = Color.Lerp(startColor, targetColor, smoothPercentage);
				yield return null;
			}

			image.color = targetColor;
		}

		void ChangeVisibilityInstant(bool isVisible)
		{
			canvasGroup.alpha = isVisible ? 1f : 0f;
			this.isVisible = isVisible;
		}
		IEnumerator ChangeVisibility(bool isVisible, float speed)
		{
			yield return FadeImage(canvasGroup, isVisible, speed);

			this.isVisible = isVisible;
			visibilityCoroutine = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed)
		{
			Vector2 startPos = root.anchoredPosition;
			Vector2 endPos = GetTargetPosition(normalizedPos);
			float distance = Vector2.Distance(startPos, endPos);
			speed = speed <= 0 ? manager.GameOptions.Characters.MoveSpeed : speed;

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

		Color GetDarkColor(Color lightColor)
		{
			return new Color(
				lightColor.r * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.g * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.b * manager.GameOptions.Characters.DarkenBrightness,
				lightColor.a
			);
		}
	}
}
