using System.Collections;
using UnityEngine;

namespace Characters
{
	public class SpriteCharacter : Character
	{
		const float MoveSpeedMultiplier = 100000f;

		RectTransform root;
		Coroutine visibilityProcess;
		Coroutine moveProcess;

		public SpriteCharacter(CharacterManager characterManager, CharacterData data, GameObject rootGameObject)
			: base(characterManager, data)
		{
			InitRoot(rootGameObject);

			Debug.Log($"Created Sprite Character: {data.Name}");
		}

		void InitRoot(GameObject rootGameObject)
		{
			root = rootGameObject.GetComponent<RectTransform>();
			CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
		}

		public override void SetPosition(Vector2 normalizedPos)
		{
			root.position = GetTargetPosition(normalizedPos);
		}

		public override Coroutine MoveToPosition(Vector2 normalizedPos, float speed)
		{
			StopProcess(ref moveProcess);

			moveProcess = Manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
			return moveProcess;
		}

		public override Coroutine Show()
		{
			StopProcess(ref visibilityProcess);

			visibilityProcess = Manager.StartCoroutine(ChangeVisibility(true));
			return visibilityProcess;
		}

		public override Coroutine Hide()
		{
			StopProcess(ref visibilityProcess);

			visibilityProcess = Manager.StartCoroutine(ChangeVisibility(false));
			return visibilityProcess;
		}

		IEnumerator ChangeVisibility(bool isShowing)
		{
			float targetAlpha = isShowing ? 1f : 0f;
			float visibilityChangeSpeed = isShowing ? Manager.GameOptions.CharacterShowSpeed : Manager.GameOptions.CharacterHideSpeed;

			CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();

			while (canvasGroup.alpha != targetAlpha)
			{
				canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, visibilityChangeSpeed * Time.deltaTime);
				yield return null;
			}

			visibilityProcess = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed)
		{
			Vector2 startPos = root.position;
			Vector2 endPos = GetTargetPosition(normalizedPos);
			float sqrDistance = (endPos - startPos).sqrMagnitude;

			// Move at a constant speed towards the target location
			float distancePercent = 0f;
			while (distancePercent < 1f)
			{
				distancePercent += (speed * MoveSpeedMultiplier * Time.deltaTime) / sqrDistance;
				distancePercent = Mathf.Clamp01(distancePercent);
				root.position = Vector2.Lerp(startPos, endPos, distancePercent);

				yield return null;
			}

			root.position = endPos;
			moveProcess = null;
		}

		Vector2 GetTargetPosition(Vector2 normalizedPos)
		{
			Vector2 parentSize = Manager.Container.rect.size;

			Vector2 imageOffset = (root.pivot * root.rect.size);
			Vector2 minPos = Vector2.zero + imageOffset;
			Vector2 maxPos = parentSize - imageOffset;

			Vector2 targetPos = normalizedPos * parentSize;
			float clampedX = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
			float clampedY = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);

			return new Vector2(clampedX, clampedY);
		}

		void StopProcess(ref Coroutine process)
		{
			if (process == null) return;

			Manager.StopCoroutine(process);
			process = null;
		}
	}
}
