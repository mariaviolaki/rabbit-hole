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

		protected UITransitionHandler TransitionHandler { get; private set; }
		protected Color DisplayColor { get { return isHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; set; } = Color.white;
		protected Color DarkColor { get { return GetDarkColor(LightColor); } }

		public abstract Coroutine Flip(bool isImmediate = false, float speed = 0);
		public abstract Coroutine Highlight(bool isImmediate = false, float speed = 0);
		public abstract Coroutine Unhighlight(bool isImmediate = false, float speed = 0);
		public abstract Coroutine SetColor(Color color, bool isImmediate = false, float speed = 0);

		protected override IEnumerator Init()
		{
			// Load this character's prefab into the scene
			yield return manager.FileManager.LoadCharacterPrefab(data.CastName);

			GameObject prefab = manager.FileManager.GetCharacterPrefab(data.CastName);
			if (prefab == null) yield break;

			TransitionHandler = new UITransitionHandler(manager.GameOptions);
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

		public Coroutine Show(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref visibilityCoroutine);

			if (isImmediate)
			{
				SetVisibility(true);
				return null;
			}
			else
			{
				visibilityCoroutine = manager.StartCoroutine(TransitionVisibility(true, speed, isSkipped));
				return visibilityCoroutine;
			}
		}
		
		public Coroutine Hide(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref visibilityCoroutine);

			if (isImmediate)
			{
				SetVisibility(false);
				return null;
			}
			else
			{
				visibilityCoroutine = manager.StartCoroutine(TransitionVisibility(false, speed, isSkipped));
				return visibilityCoroutine;
			}
		}

		public Coroutine SetPosition(float x, float y, bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref moveCoroutine);

			if (float.IsNaN(x)) x = currentPos.x;
			if (float.IsNaN(y)) y = currentPos.y;
			currentPos = new Vector2(x, y);

			if (isImmediate)
			{
				root.anchoredPosition = GetTargetPosition(currentPos);
				return null;
			}
			else
			{
				moveCoroutine = manager.StartCoroutine(TransitionPosition(currentPos, speed, isSkipped));
				return moveCoroutine;
			}
		}

		public Coroutine FaceLeft(bool isImmediate = false, float speed = 0)
		{
			if (!isFacingRight) return null;
			return Flip(isImmediate, speed);
		}

		public Coroutine FaceRight(bool isImmediate = false, float speed = 0)
		{
			if (isFacingRight) return null;
			return Flip(isImmediate, speed);
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
			if (isTransitionSkipped || manager.Dialogue.ReadMode == Dialogue.DialogueReadMode.Skip)
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

		void SetVisibility(bool isVisible)
		{
			rootCanvasGroup.alpha = isVisible ? 1f : 0f;
			this.isVisible = isVisible;
		}

		IEnumerator TransitionVisibility(bool isVisible, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.FadeTransitionSpeed, isSkipped);

			yield return TransitionHandler.SetVisibility(rootCanvasGroup, isVisible, speed);

			this.isVisible = isVisible;
			visibilityCoroutine = null;
		}

		IEnumerator TransitionPosition(Vector2 normalizedPos, float speed, bool isSkipped)
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

			Vector2 pivotOffset = root.pivot * rootSize;
			Vector2 minPos = pivotOffset;
			Vector2 maxPos = containerSize - rootSize + pivotOffset;

			// Clamp the target pos so the sprite stays inside
			if (normalizedPos.x >= 0f && normalizedPos.x <= 1f)
				targetPos.x = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
			if (normalizedPos.y >= 0f && normalizedPos.y <= 1f)
				targetPos.y = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);

			// Convert anchor to offset from bottom-left of container
			Vector2 anchorCenter = (root.anchorMin + root.anchorMax) * 0.5f;
			Vector2 anchorOffset = Vector2.Scale(anchorCenter, containerSize);

			// Return position relative to anchor center
			return targetPos - anchorOffset;
		}
	}
}
