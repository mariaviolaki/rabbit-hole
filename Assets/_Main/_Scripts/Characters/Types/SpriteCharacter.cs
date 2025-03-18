using System.Collections;
using UnityEngine;

namespace Characters
{
	public class SpriteCharacter : Character
	{
		RectTransform root;
		Coroutine visibilityProcess;

		public SpriteCharacter(CharacterManager characterManager, CharacterData data, GameObject rootGameObject)
			: base(characterManager, data)
		{
			InitRoot(rootGameObject);
			Debug.Log($"Created Sprite Character: {data.Name}");
		}

		void InitRoot(GameObject rootGameObject)
		{
			if (rootGameObject == null) return;

			root = rootGameObject.GetComponent<RectTransform>();
			CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
		}

		public override Coroutine Show()
		{
			if (root == null) return null;

			if (visibilityProcess != null)
				Manager.StopCoroutine(visibilityProcess);

			visibilityProcess = Manager.StartCoroutine(ChangeVisibility(true));
			return visibilityProcess;
		}

		public override Coroutine Hide()
		{
			if (root == null) return null;

			if (visibilityProcess != null)
				Manager.StopCoroutine(visibilityProcess);

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
		}
	}
}
