using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class SideMenuUI : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] RectTransform root;
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] float closedPositionOffset;

		const float MoveAnimationSpeedMultiplier = 0.00001f;

		UITransitionHandler transitionHandler;
		Vector2 openPosition;
		bool isTransitioning = false;

		public event Action OnClose;

		public bool IsTransitioning => isTransitioning;
		public bool IsVisible => canvasGroup.alpha == 1f;
		public bool IsHidden => canvasGroup.alpha == 0f;

		void Awake()
		{
			transitionHandler = new UITransitionHandler(gameOptions);

			Initialize();
			SetHiddenImmediate();
		}

		void OnEnable()
		{
			inputManager.CurrentMenu = MenuType.SideMenu;
		}

		void OnDisable()
		{
			inputManager.CurrentMenu = MenuType.None;
		}

		public void SetVisibleImmediate() => canvasGroup.alpha = 1f;
		public void SetHiddenImmediate() => canvasGroup.alpha = 0f;

		public void OpenDefault() => StartCoroutine(Open());
		public IEnumerator Open(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;
			yield return OpenProcess(isImmediate, transitionSpeed);

			isTransitioning = false;
		}

		public void CloseDefault() => StartCoroutine(Close());
		public IEnumerator Close(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;
			yield return CloseProcess(isImmediate, transitionSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		IEnumerator OpenProcess(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetVisibleImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				Vector2 closedPosition = new(openPosition.x + closedPositionOffset, openPosition.y);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, true, speed),
					MoveToPosition(closedPosition, openPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}
		}

		IEnumerator CloseProcess(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetHiddenImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				Vector2 closedPosition = new(openPosition.x + closedPositionOffset, openPosition.y);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, false, speed),
					MoveToPosition(openPosition, closedPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}
		}

		IEnumerator MoveToPosition(Vector2 startPos, Vector2 endPos, float speed)
		{
			float distance = (endPos - startPos).sqrMagnitude;
			float duration = (1 / speed) * MoveAnimationSpeedMultiplier * distance;

			if (duration <= 0) yield break;

			float progress = 0f;
			while (progress < duration)
			{
				progress += Time.deltaTime;

				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));
				root.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);

				yield return null;
			}

			root.anchoredPosition = endPos;
		}

		void Initialize()
		{
			// Initialize the start position of the side menu
			Canvas.ForceUpdateCanvases();
			openPosition = root.anchoredPosition;
		}
	}
}