using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UI
{
	public class ChoicePanelUI : BaseFadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] ChoiceButtonUI choiceButtonPrefab;

		ObjectPool<ChoiceButtonUI> buttonPool;
		List<ChoiceButtonUI> activeButtons = new();

		const int poolSize = 5;
		bool isImmediate = false;
		int lastChoice = -1;

		public int LastChoice => lastChoice;

		public event Action OnClose;

		protected override void Awake()
		{
			base.Awake();

			buttonPool = new ObjectPool<ChoiceButtonUI>(OnCreateButton, OnGetButton, OnReleaseButton, OnDestroyButton, maxSize: poolSize);
		}

		public Coroutine Show(string[] choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible) return null;

			base.fadeSpeed = fadeSpeed;
			this.isImmediate = isImmediate;

			PrepareChoicePanel(choices);

			return SetVisible(isImmediate, fadeSpeed);
		}

		Coroutine Hide()
		{
			if (IsHidden) return null;

			return SetHidden(isImmediate, fadeSpeed);
		}

		void PrepareChoicePanel(string[] choices)
		{
			CreateButtons(choices);
			inputManager.IsChoicePanelOpen = true;
		}

		void CreateButtons(string[] choices)
		{
			for (int i = 0; i < choices.Length; i++)
			{
				ChoiceButtonUI choiceButton = buttonPool.Get();
				choiceButton.UpdateData(i, choices[i]);
			}
		}

		void SelectChoice(int index, string text)
		{
			Debug.Log($"Button {index} selected: {text}");

			lastChoice = index;

			inputManager.IsChoicePanelOpen = false;
			inputManager.OnSelectChoice?.Invoke(index, text);

			StartCoroutine(HideAndClear());
		}

		ChoiceButtonUI OnCreateButton()
		{
			ChoiceButtonUI choiceButtonUI = Instantiate(choiceButtonPrefab, transform);
			choiceButtonUI.Initialize(buttonPool);
			return choiceButtonUI;
		}

		void OnGetButton(ChoiceButtonUI choiceButton)
		{
			choiceButton.OnSelect += SelectChoice;
			choiceButton.ClearData();
			choiceButton.gameObject.SetActive(true);
			choiceButton.Show(isImmediate, fadeSpeed);
			activeButtons.Add(choiceButton);
		}

		void OnReleaseButton(ChoiceButtonUI choiceButton)
		{
			choiceButton.gameObject.SetActive(false);
			activeButtons.Remove(choiceButton);
		}

		void OnDestroyButton(ChoiceButtonUI choiceButton)
		{
			Destroy(choiceButton.gameObject);
		}

		IEnumerator HideAndClear()
		{
			foreach (ChoiceButtonUI choiceButton in activeButtons)
			{
				choiceButton.RemoveListeners();
				choiceButton.OnSelect -= SelectChoice;
			}

			yield return Hide();

			for (int i = activeButtons.Count - 1; i >= 0; i--)
				activeButtons[i].Release();

			OnClose?.Invoke();
		}
	}
}
