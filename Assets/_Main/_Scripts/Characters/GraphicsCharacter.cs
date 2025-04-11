using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		protected const float MoveSpeedMultiplier = 100f;

		Coroutine moveCoroutine;
		protected Coroutine visibilityCoroutine;

		protected CanvasGroup canvasGroup;
		protected Animator animator;
		protected bool isFacingRight;
		protected bool isHighlighted = true;

		protected Color DisplayColor { get { return isHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; private set; } = Color.white;
		protected Color DarkColor
		{
			get
			{
				return new Color(LightColor.r * manager.GameOptions.DarkenBrightness,
					LightColor.g * manager.GameOptions.DarkenBrightness,
					LightColor.b * manager.GameOptions.DarkenBrightness,
					LightColor.a);
			}
		}

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
			isFacingRight = manager.GameOptions.AreSpritesFacingRight;
		}

		public abstract Coroutine Flip(float speed = 0);
		public abstract Coroutine FaceLeft(float speed = 0);
		public abstract Coroutine FaceRight(float speed = 0);
		public abstract Coroutine Lighten(float speed = 0);
		public abstract Coroutine Darken(float speed = 0);

		public virtual Coroutine SetColor(Color color, float speed = 0)
		{
			LightColor = color;
			return null;
		}

		public void SetPriority(int index)
		{
			if (!isVisible) return;

			manager.SetPriority(data.Name, index);
		}

		public void Animate(string animationName)
		{
			animator.SetTrigger(animationName);
		}

		public void Animate(string animationName, bool isPlaying)
		{
			animator.SetBool(animationName, isPlaying);
		}

		public void SetPosition(Vector2 normalizedPos)
		{
			root.anchoredPosition = GetTargetPosition(normalizedPos);
		}

		public Coroutine Show()
		{
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(true));
			return visibilityCoroutine;
		}

		public Coroutine Hide()
		{
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(false));
			return visibilityCoroutine;
		}

		public Coroutine MoveToPosition(Vector2 normalizedPos, float speed)
		{
			manager.StopProcess(ref moveCoroutine);

			moveCoroutine = manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
			return moveCoroutine;
		}

		IEnumerator ChangeVisibility(bool isVisible)
		{
			float targetAlpha = isVisible ? 1f : 0f;
			float visibilityChangeSpeed = isVisible ? manager.GameOptions.CharacterShowSpeed : manager.GameOptions.CharacterHideSpeed;

			while (canvasGroup.alpha != targetAlpha)
			{
				canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, visibilityChangeSpeed * Time.deltaTime);
				yield return null;
			}

			this.isVisible = isVisible;
			visibilityCoroutine = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed)
		{
			Vector2 startPos = root.position;
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
				root.position = Vector2.Lerp(startPos, endPos, smoothDistance);

				yield return null;
			}

			root.position = endPos;
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
