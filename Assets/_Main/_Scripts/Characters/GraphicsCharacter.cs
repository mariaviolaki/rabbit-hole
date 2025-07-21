using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		const float MoveSpeedMultiplier = 135000f;

		readonly Dictionary<string, string> animationNames = new(StringComparer.OrdinalIgnoreCase);

		Vector2 currentPos = Vector2.zero;
		protected CanvasGroup rootCanvasGroup;
		protected Animator animator;
		protected bool isFacingRight;
		protected bool isHighlighted = true;

		Coroutine moveCoroutine;
		protected Coroutine directionCoroutine;
		protected Coroutine visibilityCoroutine;
		protected Coroutine colorCoroutine;
		protected Coroutine brightnessCoroutine;

		Coroutine skippedMoveCoroutine;
		protected Coroutine skippedDirectionCoroutine;
		protected Coroutine skippedVisibilityCoroutine;
		protected Coroutine skippedColorCoroutine;
		protected Coroutine skippedBrightnessCoroutine;

		protected UITransitionHandler TransitionHandler { get; private set; }
		public Color LightColor { get; protected set; } = Color.white;
		public Color DisplayColor => isHighlighted ? LightColor : DarkColor;
		protected Color DarkColor => GetDarkColor(LightColor);
		public int HierarchyPriority => root.GetSiblingIndex();
		public Vector2 Position => currentPos;
		public bool IsHighlighted => isHighlighted;
		public bool IsFacingRight => isFacingRight;
		public Animator RootAnimator => animator;

		public abstract Coroutine Flip(bool isImmediate = false, float speed = 0);
		public abstract Coroutine SetHighlighted(bool isHighlighted, bool isImmediate = false, float speed = 0);
		public abstract Coroutine SetColor(Color color, bool isImmediate = false, float speed = 0);

		protected override IEnumerator Init()
		{
			// Load this character's prefab into the scene
			yield return manager.FileManager.LoadCharacterPrefab(data.CastName);

			GameObject prefab = manager.FileManager.GetCharacterPrefab(data.CastName);
			if (prefab == null) yield break;

			TransitionHandler = new UITransitionHandler(manager.GameOptions);
			GameObject rootGameObject = UnityEngine.Object.Instantiate(prefab, manager.Container);
			root = rootGameObject.GetComponent<RectTransform>();
			rootCanvasGroup = root.GetComponent<CanvasGroup>();
			animator = root.GetComponentInChildren<Animator>();

			root.name = data.Name;
			rootCanvasGroup.alpha = 0f;
			isFacingRight = manager.GameOptions.Characters.AreSpritesFacingRight;
			CacheAnimationNames();
		}

		public Coroutine Highlight(bool isImmediate = false, float speed = 0) => SetHighlighted(true, isImmediate, speed);
		public Coroutine Unhighlight(bool isImmediate = false, float speed = 0) => SetHighlighted(false, isImmediate, speed);
		public Coroutine Show(bool isImmediate = false, float speed = 0) => SetVisibility(true, isImmediate, speed);
		public Coroutine Hide(bool isImmediate = false, float speed = 0) => SetVisibility(false, isImmediate, speed);

		public void SetPriority(int index)
		{
			if (!isVisible) return;

			manager.SetPriority(data.ShortName, index);
		}

		public Coroutine SetVisibility(bool isVisible, bool isImmediate = false, float speed = 0)
		{
			if (isVisible == this.isVisible || skippedVisibilityCoroutine != null) return null;
			bool isSkipped = manager.StopProcess(ref visibilityCoroutine);

			if (isImmediate)
			{
				SetVisibilityImmediate(isVisible);
				return null;
			}
			else if (isSkipped)
			{
				skippedVisibilityCoroutine = manager.StartCoroutine(TransitionVisibility(isVisible, speed, isSkipped));
				return skippedVisibilityCoroutine;
			}
			else
			{
				visibilityCoroutine = manager.StartCoroutine(TransitionVisibility(isVisible, speed, isSkipped));
				return visibilityCoroutine;
			}
		}

		public Coroutine SetPosition(float x, float y, bool isImmediate = false, float speed = 0)
		{
			if (float.IsNaN(x)) x = currentPos.x;
			if (float.IsNaN(y)) y = currentPos.y;
			Vector2 inputPos = new(x, y);

			if (Utilities.AreApproximatelyEqual(inputPos, currentPos) || skippedMoveCoroutine != null) return null;
			bool isSkipped = manager.StopProcess(ref moveCoroutine);

			if (isImmediate)
			{
				SetPositionImmediate(inputPos);
				return null;
			}
			else if (isSkipped)
			{
				skippedMoveCoroutine = manager.StartCoroutine(TransitionPosition(inputPos, speed, isSkipped));
				return skippedMoveCoroutine;
			}
			else
			{
				moveCoroutine = manager.StartCoroutine(TransitionPosition(inputPos, speed, isSkipped));
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
			if (!animationNames.TryGetValue(animationName, out string cachedName))
			{
				Debug.LogWarning($"Animation '{animationName}' not found for character '{data.Name}'.");
				return;
			}

			animator.SetTrigger(cachedName);
		}

		public void SetAnimation(string animationName, bool isPlaying)
		{
			if (!animationNames.TryGetValue(animationName, out string cachedName))
			{
				Debug.LogWarning($"Animation '{animationName}' not found for character '{data.Name}'.");
				return;
			}

			animator.SetBool(cachedName, isPlaying);
		}

		void SetPositionImmediate(Vector2 inputPos)
		{
			Vector2 targetPos = GetTargetPosition(inputPos);
			root.anchoredPosition = targetPos;
			currentPos = inputPos;
		}

		void SetVisibilityImmediate(bool isVisible)
		{
			rootCanvasGroup.alpha = isVisible ? 1f : 0f;
			this.isVisible = isVisible;
		}

		IEnumerator TransitionVisibility(bool isVisible, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.TransitionSpeed, isSkipped);

			yield return TransitionHandler.SetVisibility(rootCanvasGroup, isVisible, speed);
			this.isVisible = isVisible;

			if (isSkipped) skippedVisibilityCoroutine = null;
			else visibilityCoroutine = null;
		}

		IEnumerator TransitionPosition(Vector2 inputPos, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.MoveSpeed, isSkipped);

			Vector2 startPos = root.anchoredPosition;
			Vector2 endPos = GetTargetPosition(inputPos);
			float distance = (endPos - startPos).sqrMagnitude;

			if (distance <= Mathf.Epsilon)
			{
				moveCoroutine = null;
				yield break;
			}

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
			currentPos = inputPos;

			if (isSkipped) skippedMoveCoroutine = null;
			else moveCoroutine = null;
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

		public float GetTransitionSpeed(float speedInput, float defaultSpeed, bool isTransitionSkipped)
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

		void CacheAnimationNames()
		{
			foreach (AnimatorControllerParameter parameter in animator.parameters)
			{
				animationNames[parameter.name] = parameter.name;
			}
		}
	}
}
