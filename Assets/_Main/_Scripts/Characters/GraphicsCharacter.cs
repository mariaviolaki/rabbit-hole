using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		const float MoveSlowdownStartDistance = 300f;
		const float MinMoveSlowdownDistanceRatio = 0.05f;

		const float PositionSpeedMultiplier = 100f;
		const float VisibilitySpeedMultiplier = 0.5f;
		const float DirectionSpeedMultiplier = 0.2f;
		protected const float ColorSpeedMultiplier = 50f;

		readonly Dictionary<string, string> animationNames = new(StringComparer.OrdinalIgnoreCase);
		Vector2 leftDirection;
		Vector2 rightDirection;

		protected UITransitionHandler transitionHandler;
		protected CanvasGroup rootCanvasGroup;
		protected Animator animator;
		protected GameObject primaryFlipContainer;
		protected GameObject secondaryFlipContainer;
		CanvasGroup primaryFlipCanvasGroup;
		CanvasGroup secondaryFlipCanvasGroup;

		Vector2 position;
		bool isVisible;
		Vector2 direction;
		protected Color color;
		protected bool isHighlighted;

		TransitionStatus positionStatus = TransitionStatus.Completed;
		TransitionStatus visibilityStatus = TransitionStatus.Completed;
		TransitionStatus directionStatus = TransitionStatus.Completed;
		protected TransitionStatus colorStatus = TransitionStatus.Completed;

		float positionSpeed;
		float visibilitySpeed;
		float directionSpeed;
		protected float colorSpeed;

		public Animator RootAnimator => animator;
		public int HierarchyPriority => root.GetSiblingIndex();
		public Vector2 Position => position;
		public bool IsVisible => isVisible;
		public Vector2 Direction => direction;
		public Color LightColor => color;
		public bool IsHighlighted => isHighlighted;

		public bool IsTransitioningPosition() => positionStatus != TransitionStatus.Completed;
		public bool IsTransitioningVisibility() => visibilityStatus != TransitionStatus.Completed;
		public bool IsTransitioningDirection() => directionStatus != TransitionStatus.Completed;
		public bool IsTransitioningColor() => colorStatus != TransitionStatus.Completed;

		abstract protected void SetColorImmediate();
		abstract protected void TransitionColor();

		override protected void Update()
		{
			base.Update();

			TransitionPosition();
			TransitionVisibility();
			TransitionDirection();
		}

		override public IEnumerator Initialize(CharacterManager manager, CharacterData data)
		{
			yield return base.Initialize(manager, data);

			root = transform.GetComponent<RectTransform>();
			rootCanvasGroup = root.GetComponent<CanvasGroup>();
			animator = root.GetComponentInChildren<Animator>();
			primaryFlipContainer = animator.transform.GetChild(0).gameObject;
			secondaryFlipContainer = animator.transform.GetChild(1).gameObject;
			primaryFlipCanvasGroup = primaryFlipContainer.GetComponent<CanvasGroup>();
			secondaryFlipCanvasGroup = secondaryFlipContainer.GetComponent<CanvasGroup>();

			root.name = data.Name;
			primaryFlipCanvasGroup.alpha = 1f;
			secondaryFlipCanvasGroup.alpha = 0f;
			primaryFlipContainer.SetActive(true);
			secondaryFlipContainer.SetActive(false);

			CacheAnimationNames();
			InitVisuals();
		}

		public void SetPriority(int index)
		{
			if (!isVisible) return;

			manager.SetPriority(data.ShortName, index);
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

		public void SetPosition(float x, float y, bool isImmediate = false, float speed = 0)
		{
			if (float.IsNaN(x)) x = position.x;
			if (float.IsNaN(y)) y = position.y;

			Vector2 inputPosition = new(x, y);
			if (inputPosition == position && positionStatus == TransitionStatus.Completed) return;

			position = inputPosition;
			if (isImmediate)
			{
				SetPositionImmediate();
				positionStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.MoveSpeed;
				bool isSkipped = positionStatus != TransitionStatus.Completed;
				positionSpeed = GetTransitionSpeed(speed, defaultSpeed, PositionSpeedMultiplier, isSkipped);
				positionStatus = positionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void Show(bool isImmediate = false, float speed = 0) => SetVisibility(true, isImmediate, speed);
		public void Hide(bool isImmediate = false, float speed = 0) => SetVisibility(false, isImmediate, speed);
		public void SetVisibility(bool isVisible, bool isImmediate = false, float speed = 0)
		{
			if (isVisible == this.isVisible && visibilityStatus == TransitionStatus.Completed) return;

			this.isVisible = isVisible;
			if (isImmediate)
			{
				SetVisibilityImmediate();
				visibilityStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.TransitionSpeed;
				bool isSkipped = visibilityStatus != TransitionStatus.Completed;
				visibilitySpeed = GetTransitionSpeed(speed, defaultSpeed, VisibilitySpeedMultiplier, isSkipped);
				visibilityStatus = visibilityStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void FaceLeft(bool isImmediate = false, float speed = 0) => SetDirection(leftDirection, isImmediate, speed);
		public void FaceRight(bool isImmediate = false, float speed = 0) => SetDirection(rightDirection, isImmediate, speed);
		public void SetDirection(Vector2 direction, bool isImmediate = false, float speed = 0)
		{
			if (direction == this.direction && directionStatus == TransitionStatus.Completed) return;

			this.direction = direction;
			if (isImmediate)
			{
				SetDirectionImmediate();
				directionStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.TransitionSpeed;
				bool isSkipped = directionStatus != TransitionStatus.Completed;
				directionSpeed = GetTransitionSpeed(speed, defaultSpeed, DirectionSpeedMultiplier, isSkipped);
				directionStatus = directionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void SetColor(Color color, bool isImmediate = false, float speed = 0)
		{
			if (color == this.color && colorStatus == TransitionStatus.Completed) return;

			this.color = color;
			if (isImmediate)
			{
				SetColorImmediate();
				colorStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.TransitionSpeed;
				bool isSkipped = colorStatus != TransitionStatus.Completed;
				colorSpeed = GetTransitionSpeed(speed, defaultSpeed, ColorSpeedMultiplier, isSkipped);
				colorStatus = colorStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void Highlight(bool isImmediate = false, float speed = 0) => SetHighlighted(true, isImmediate, speed);
		public void Unhighlight(bool isImmediate = false, float speed = 0) => SetHighlighted(false, isImmediate, speed);
		public void SetHighlighted(bool isHighlighted, bool isImmediate = false, float speed = 0)
		{
			if (isHighlighted == this.isHighlighted && colorStatus == TransitionStatus.Completed) return;

			this.isHighlighted = isHighlighted;
			if (isImmediate)
			{
				SetColorImmediate();
				colorStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.TransitionSpeed;
				bool isSkipped = colorStatus != TransitionStatus.Completed;
				colorSpeed = GetTransitionSpeed(speed, defaultSpeed, ColorSpeedMultiplier, isSkipped);
				colorStatus = colorStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void SkipPositionTransition()
		{
			if (positionStatus == TransitionStatus.Completed) return;
			
			float defaultSpeed = vnOptions.Characters.MoveSpeed;
			positionSpeed = GetTransitionSpeed(defaultSpeed, defaultSpeed, PositionSpeedMultiplier, true);
			positionStatus = TransitionStatus.Skipped;
		}

		public void SkipVisibilityTransition()
		{
			if (visibilityStatus == TransitionStatus.Completed) return;

			float defaultSpeed = vnOptions.Characters.TransitionSpeed;
			visibilitySpeed = GetTransitionSpeed(defaultSpeed, defaultSpeed, VisibilitySpeedMultiplier, true);
			visibilityStatus = TransitionStatus.Skipped;
		}

		public void SkipDirectionTransition()
		{
			if (directionStatus == TransitionStatus.Completed) return;

			float defaultSpeed = vnOptions.Characters.TransitionSpeed;
			directionSpeed = GetTransitionSpeed(defaultSpeed, defaultSpeed, DirectionSpeedMultiplier, true);
			directionStatus = TransitionStatus.Skipped;
		}

		public void SkipColorTransition()
		{
			if (colorStatus == TransitionStatus.Completed) return;

			float defaultSpeed = vnOptions.Characters.TransitionSpeed;
			colorSpeed = GetTransitionSpeed(defaultSpeed, defaultSpeed, ColorSpeedMultiplier, true);
			colorStatus = TransitionStatus.Skipped;
		}

		void SetPositionImmediate()
		{
			Vector2 targetPos = GetTargetPosition(position);
			root.anchoredPosition = targetPos;
		}

		void SetVisibilityImmediate()
		{
			rootCanvasGroup.alpha = isVisible ? 1f : 0f;
		}

		void SetDirectionImmediate()
		{
			primaryFlipContainer.transform.localScale = direction;
			secondaryFlipContainer.transform.localScale = direction;
			primaryFlipCanvasGroup.alpha = 1f;
			secondaryFlipCanvasGroup.alpha = 0f;
			secondaryFlipContainer.SetActive(false);
		}

		void TransitionPosition()
		{
			if (positionStatus == TransitionStatus.Completed) return;

			Vector2 targetPosition = GetTargetPosition(position);
			float speed = positionSpeed * Time.deltaTime;

			// Slow down proportionally as we get closer to the target
			float distance = Vector2.Distance(root.anchoredPosition, targetPosition);
			if (distance < MoveSlowdownStartDistance)
			{
				float distanceRatio = Mathf.Max(distance / MoveSlowdownStartDistance, MinMoveSlowdownDistanceRatio);
				speed *= distanceRatio;
			}

			root.anchoredPosition = Vector2.MoveTowards(root.anchoredPosition, targetPosition, speed);

			if (Utilities.AreApproximatelyEqual(root.anchoredPosition, targetPosition))
			{
				root.anchoredPosition = targetPosition;
				positionStatus = TransitionStatus.Completed;
			}
		}

		void TransitionVisibility()
		{
			if (visibilityStatus == TransitionStatus.Completed) return;

			float targetAlpha = isVisible ? 1f : 0f;
			float speed = visibilitySpeed * Time.deltaTime;
			rootCanvasGroup.alpha = Mathf.MoveTowards(rootCanvasGroup.alpha, targetAlpha, speed);

			if (Utilities.AreApproximatelyEqual(rootCanvasGroup.alpha, targetAlpha))
			{
				rootCanvasGroup.alpha = targetAlpha;
				visibilityStatus = TransitionStatus.Completed;
			}
		}

		void TransitionDirection()
		{
			if (directionStatus == TransitionStatus.Completed) return;

			if (!secondaryFlipContainer.activeInHierarchy)
				secondaryFlipContainer.SetActive(true);

			float speed = directionSpeed * Time.deltaTime;
			secondaryFlipContainer.transform.localScale = direction;
			primaryFlipCanvasGroup.alpha = Mathf.MoveTowards(primaryFlipCanvasGroup.alpha, 0f, speed);
			secondaryFlipCanvasGroup.alpha = Mathf.MoveTowards(secondaryFlipCanvasGroup.alpha, 1f, speed);

			if (Utilities.AreApproximatelyEqual(secondaryFlipCanvasGroup.alpha, 1f))
			{
				primaryFlipContainer.transform.localScale = direction;
				primaryFlipCanvasGroup.alpha = 1f;
				secondaryFlipCanvasGroup.alpha = 0f;
				secondaryFlipContainer.SetActive(false);
				directionStatus = TransitionStatus.Completed;
			}
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

		protected Color GetDisplayColor()
		{
			if (isHighlighted) return color;

			return new Color(
				color.r * manager.Options.Characters.DarkenBrightness,
				color.g * manager.Options.Characters.DarkenBrightness,
				color.b * manager.Options.Characters.DarkenBrightness,
				color.a
			);
		}

		public float GetTransitionSpeed(float speedInput, float defaultSpeed, float speedMultiplier, bool isTransitionSkipped)
		{
			float baseSpeed = speedInput;

			if (isTransitionSkipped || manager.Dialogue.FlowController.IsSkipping)
				baseSpeed = manager.Options.General.SkipTransitionSpeed;
			else if (speedInput < Mathf.Epsilon)
				baseSpeed = defaultSpeed;

			return baseSpeed * speedMultiplier;
		}

		void CacheAnimationNames()
		{
			foreach (AnimatorControllerParameter parameter in animator.parameters)
			{
				animationNames[parameter.name] = parameter.name;
			}
		}

		void InitVisuals()
		{
			// Initialize Position
			Vector2 startPosition = new Vector2(-1, -1);
			position = startPosition;
			SetPositionImmediate();

			// Initialize Visibility
			isVisible = false;
			SetVisibilityImmediate();

			// Initialize Direction
			Vector2 currentDirection = primaryFlipContainer.transform.localScale;
			float rightX = manager.Options.Characters.AreSpritesFacingRight ? currentDirection.x : -currentDirection.x;
			leftDirection = new(-rightX, currentDirection.y);
			rightDirection = new(rightX, currentDirection.y);
			direction = currentDirection;
			SetDirectionImmediate();

			// Initialize Color
			color = Color.white;
			isHighlighted = !manager.Options.Characters.HighlightOnSpeak;
		}
	}
}
