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
		DialogueChoice lastChoice;
		Coroutine visibilityCoroutine;

		public DialogueChoice LastChoice => lastChoice;

		public event Action OnClose;

		protected override void Awake()
		{
			base.Awake();
			buttonPool = new ObjectPool<ChoiceButtonUI>(OnCreateButton, OnGetButton, OnReleaseButton, OnDestroyButton, maxSize: poolSize);
		}

		override protected void OnEnable()
		{
			base.OnEnable();
			PrepareOpen();
		}

		override protected void OnDisable()
		{
			base.OnDisable();
			CompleteClose();
		}

		public Coroutine Show(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible) return null;

			base.fadeSpeed = fadeSpeed;
			StopProcess();

			visibilityCoroutine = StartCoroutine(ShowProcess(choices, isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		public Coroutine ForceHide(bool isImmediate = false)
		{
			fadeSpeed = gameOptions.General.SkipTransitionSpeed;

			StopProcess();
			visibilityCoroutine = StartCoroutine(HideProcess(isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		public IEnumerator OpenProcess(List<DialogueChoice> choices, bool isImmediate = false, float speed = 0)
		{
			CreateButtons(choices);

			yield return FadeIn(isImmediate, speed);
		}

		public IEnumerator CloseProcess(bool isImmediate = false, float speed = 0)
		{
			foreach (ChoiceButtonUI choiceButton in activeButtons)
			{
				choiceButton.RemoveListeners();
				choiceButton.OnSelect -= SelectChoice;
			}

			yield return FadeOut(isImmediate, speed);

			for (int i = activeButtons.Count - 1; i >= 0; i--)
				activeButtons[i].Release();

			if (lastChoice != null)
				inputManager.OnSelectChoice?.Invoke(lastChoice);

			OnClose?.Invoke();
		}

		IEnumerator ShowProcess(List<DialogueChoice> choices, bool isImmediate = false, float speed = 0)
		{
			yield return OpenProcess(choices, isImmediate, speed);
			visibilityCoroutine = null;
		}

		IEnumerator HideProcess(bool isImmediate = false, float speed = 0)
		{
			yield return CloseProcess(isImmediate, speed);
			visibilityCoroutine = null;
		}

		void PrepareOpen()
		{
			lastChoice = null;
			inputManager.IsChoicePanelOpen = true;
			inputManager.OnClearChoice?.Invoke();
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

		void SelectChoice(DialogueChoice choice)
		{
			lastChoice = choice;
			StopProcess();
			visibilityCoroutine = StartCoroutine(HideProcess(isImmediateTransition, fadeSpeed));
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
			choiceButton.OnSelect += SelectChoice;
			choiceButton.ClearData();
			choiceButton.gameObject.SetActive(true);
			choiceButton.Show(true);
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

		void StopProcess()
		{
			if (visibilityCoroutine == null) return;

			StopCoroutine(visibilityCoroutine);
			visibilityCoroutine = null;
		}
	}
}
