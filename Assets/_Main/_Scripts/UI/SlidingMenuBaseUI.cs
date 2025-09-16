using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class SlidingMenuBaseUI : MenuBaseUI
	{
		[SerializeField] protected RectTransform slideRoot;
		[SerializeField] protected float closedPositionOffset;

		protected float moveAnimationSpeedMultiplier = 1f;
		protected Vector2 openPosition = Vector2.zero;
		protected Vector2 closedPosition = Vector2.zero;

		override protected void Start()
		{
			StartCoroutine(Initialize());
		}

		virtual protected IEnumerator Initialize()
		{
			SubscribeListeners();
			OnClose?.Invoke();
			yield break;
		}

		override public IEnumerator SetVisible(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetVisibleImmediate();
			}
			else
			{
				speed = speed < Mathf.Epsilon ? vnOptions.General.TransitionSpeed : speed;

				List<IEnumerator> transitionProcesses = new()
			{
				transitionHandler.SetVisibility(canvasGroup, true, speed),
				MoveToPosition(closedPosition, openPosition, speed)
			};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}
		}

		override public IEnumerator SetHidden(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetHiddenImmediate();
			}
			else
			{
				speed = speed < Mathf.Epsilon ? vnOptions.General.TransitionSpeed : speed;

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
			float duration = 1 / speed * moveAnimationSpeedMultiplier * distance;

			if (duration <= 0) yield break;

			float progress = 0f;
			while (progress < duration)
			{
				progress += Time.deltaTime;

				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));
				slideRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);

				yield return null;
			}

			slideRoot.anchoredPosition = endPos;
		}
	}
}
