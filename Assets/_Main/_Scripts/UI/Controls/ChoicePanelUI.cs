using Logic;
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
		readonly List<ChoiceButtonUI> activeButtons = new();

		const int poolSize = 5;
		bool isImmediate = false;
		DialogueChoice lastChoice;

		public DialogueChoice LastChoice => lastChoice;

		public event Action OnClose;

		protected override void Awake()
		{
			base.Awake();

			buttonPool = new ObjectPool<ChoiceButtonUI>(OnCreateButton, OnGetButton, OnReleaseButton, OnDestroyButton, maxSize: poolSize);
		}

		public Coroutine Show(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
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

		void PrepareChoicePanel(List<DialogueChoice> choices)
		{
			lastChoice = null;
			CreateButtons(choices);
			inputManager.IsChoicePanelOpen = true;
			inputManager.OnClearChoice?.Invoke();
		}

		void CreateButtons(List<DialogueChoice> choices)
		{
			for (int i = 0; i < choices.Count; i++)
			{
				ChoiceButtonUI choiceButton = buttonPool.Get();
				choiceButton.UpdateChoice(choices[i]);
			}
		}

		void SelectChoice(DialogueChoice choice)
		{
			lastChoice = choice;

			inputManager.IsChoicePanelOpen = false;
			inputManager.OnSelectChoice?.Invoke(choice);

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
