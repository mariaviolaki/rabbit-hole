using Dialogue;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UI
{
	public class ChoicePanelUI : FadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] ChoiceButtonUI choiceButtonPrefab;

		ObjectPool<ChoiceButtonUI> buttonPool;
		readonly List<ChoiceButtonUI> activeButtons = new();

		const int poolSize = 5;
		DialogueChoice lastChoice;
		bool isTransitioning = false;

		public DialogueChoice LastChoice => lastChoice;

		public event Action OnClose;

		protected override void Awake()
		{
			base.Awake();
			buttonPool = new ObjectPool<ChoiceButtonUI>(OnCreateButton, OnGetButton, OnReleaseButton, OnDestroyButton, maxSize: poolSize);
		}

		void OnEnable()
		{
			PrepareOpen();
		}

		void OnDisable()
		{
			CompleteClose();
		}

		public IEnumerator Open(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			base.isImmediateTransition = isImmediate;

			CreateButtons(choices);
			yield return SetVisible(isImmediate, fadeSpeed);
			EnableButtonListeners();

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			fadeSpeed = fadeSpeed <= 0 ? vnOptions.General.SkipTransitionSpeed : fadeSpeed;

			foreach (ChoiceButtonUI choiceButton in activeButtons)
			{
				choiceButton.DisableListeners();
				choiceButton.OnSelect -= SelectChoice;
			}

			yield return SetHidden(isImmediate, fadeSpeed);

			for (int i = activeButtons.Count - 1; i >= 0; i--)
				activeButtons[i].Release();

			if (lastChoice != null)
				inputManager.TriggerSelectChoice(lastChoice);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		void PrepareOpen()
		{
			lastChoice = null;
			inputManager.IsChoicePanelOpen = true;
			inputManager.TriggerClearChoice();
		}

		void CompleteClose()
		{
			inputManager.IsChoicePanelOpen = false;
		}

		void CreateButtons(List<DialogueChoice> choices)
		{
			for (int i = 0; i < choices.Count; i++)
			{
				ChoiceButtonUI choiceButton = buttonPool.Get();
				choiceButton.UpdateChoice(choices[i]);
			}
		}

		void EnableButtonListeners()
		{
			for (int i = 0; i < activeButtons.Count; i++)
			{
				activeButtons[i].OnSelect += SelectChoice;
				activeButtons[i].EnableListeners();
			}
		}

		void SelectChoice(DialogueChoice choice)
		{
			if (isTransitioning) return;

			lastChoice = choice;
			StartCoroutine(Close(isImmediateTransition));
		}

		ChoiceButtonUI OnCreateButton()
		{
			ChoiceButtonUI choiceButtonUI = Instantiate(choiceButtonPrefab, transform);
			choiceButtonUI.Initialize(buttonPool);
			return choiceButtonUI;
		}

		void OnGetButton(ChoiceButtonUI choiceButton)
		{
			choiceButton.transform.SetParent(transform, false);
			choiceButton.ClearData();
			choiceButton.gameObject.SetActive(true);
			StartCoroutine(choiceButton.SetVisible(true));
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
	}
}
