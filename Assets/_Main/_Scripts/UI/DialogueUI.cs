using Characters;
using Dialogue;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] FontDirectorySO fontDirectory;
	[SerializeField] GameObject root;
	[SerializeField] GameObject nameContainer;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI dialogueText;

	const float rootFadeMultiplier = 1f;
	const float nameFadeMultiplier = 1f;
	CanvasGroup rootCanvasGroup;
	CanvasGroup nameCanvasGroup;
	Coroutine rootFadeCoroutine;
	Coroutine nameFadeCoroutine;

	public TextMeshProUGUI DialogueText { get { return dialogueText; } }

	void Awake()
	{
		rootCanvasGroup = root.GetComponent<CanvasGroup>();
		nameCanvasGroup = nameContainer.GetComponent<CanvasGroup>();

		rootCanvasGroup.alpha = 0f;
	}

	void Start()
	{
		UpdateFontSize(gameOptions.Dialogue.DefaultFont);
	}

	public Coroutine ShowDialogueBox(float fadeSpeed = 0)
	{
		StopFadeCoroutine(ref rootFadeCoroutine);

		if (rootCanvasGroup.alpha == 1f) return null;

		float speed = GetFadeSpeed(fadeSpeed, gameOptions.Dialogue.FadeTransitionSpeed);
		rootFadeCoroutine = StartCoroutine(FadeContainer(rootCanvasGroup, 1f, speed, rootFadeMultiplier, () => rootFadeCoroutine = null));
		return rootFadeCoroutine;
	}

	public Coroutine HideDialogueBox(float fadeSpeed = 0)
	{
		StopFadeCoroutine(ref rootFadeCoroutine);

		if (rootCanvasGroup.alpha == 0f) return null;

		float speed = GetFadeSpeed(fadeSpeed, gameOptions.Dialogue.FadeTransitionSpeed);
		rootFadeCoroutine = StartCoroutine(FadeContainer(rootCanvasGroup, 0f, speed, rootFadeMultiplier, () => rootFadeCoroutine = null));
		return rootFadeCoroutine;
	}

	public Coroutine ShowSpeaker(CharacterData characterData, float fadeSpeed = 0)
	{
		StopFadeCoroutine(ref nameFadeCoroutine);

		// Ensure that the dialogue is fully visible when a character speaks
		if (rootCanvasGroup.alpha < 1f)
			ShowDialogueBox();

		UpdateNameText(characterData);
		UpdateDialogueText(characterData);

		if (nameCanvasGroup.alpha == 1f) return null;

		float speed = GetFadeSpeed(fadeSpeed, gameOptions.Dialogue.FadeTransitionSpeed);
		nameFadeCoroutine = StartCoroutine(FadeContainer(nameCanvasGroup, 1f, speed, nameFadeMultiplier, () => nameFadeCoroutine = null));
		return nameFadeCoroutine;
	}

	public Coroutine HideSpeaker(float fadeSpeed = 0)
	{
		StopFadeCoroutine(ref nameFadeCoroutine);

		UpdateDialogueText(null);

		if (nameCanvasGroup.alpha == 0f) return null;

		float speed = GetFadeSpeed(fadeSpeed, gameOptions.Dialogue.FadeTransitionSpeed);
		nameFadeCoroutine = StartCoroutine(FadeContainer(nameCanvasGroup, 0f, speed, nameFadeMultiplier, () => nameFadeCoroutine = null));
		return nameFadeCoroutine;
	}

	void UpdateNameText(CharacterData characterData)
	{
		if (characterData == null)
		{
			nameText.text = "";
			nameText.color = gameOptions.Dialogue.DefaultTextColor;
			nameText.font = gameOptions.Dialogue.DefaultFont;
		}
		else
		{
			nameText.text = characterData.DisplayName;
			nameText.color = characterData.NameColor;
			nameText.font = characterData.NameFont;
		}
	}

	void UpdateDialogueText(CharacterData characterData)
	{
		if (characterData == null)
		{
			dialogueText.color = gameOptions.Dialogue.DefaultTextColor;
			dialogueText.font = gameOptions.Dialogue.DefaultFont;
			UpdateFontSize(gameOptions.Dialogue.DefaultFont);
		}
		else
		{
			dialogueText.color = characterData.DialogueColor;
			dialogueText.font = characterData.DialogueFont;
			UpdateFontSize(characterData.DialogueFont);
		}
	}

	void UpdateFontSize(TMP_FontAsset font)
	{
		float baseFontSize = gameOptions.Dialogue.DialogueFontSize;

		float fontSizeMultiplier = 1f;
		if (fontDirectory.Fonts.TryGetValue(font.name, out FontData fontData))
			fontSizeMultiplier = fontData.SizeMultiplier;

		dialogueText.fontSize = baseFontSize * fontSizeMultiplier;
	}

	void StopFadeCoroutine(ref Coroutine fadeCoroutine)
	{
		if (fadeCoroutine == null) return; 

		StopCoroutine(fadeCoroutine);
		fadeCoroutine = null;
	}

	float GetFadeSpeed(float speedInput, float defaultSpeed)
	{
		return (speedInput < Mathf.Epsilon) ? defaultSpeed : speedInput;
	}

	IEnumerator FadeContainer(CanvasGroup canvasGroup, float targetAlpha, float speed, float speedMultiplier, Action OnComplete)
	{
		float startAlpha = canvasGroup.alpha;

		float duration = (1f / speed) * speedMultiplier * (Mathf.Abs(targetAlpha - startAlpha));
		float progress = 0f;

		while (progress < duration)
		{
			progress += Time.deltaTime;
			float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothProgress);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha;
		OnComplete?.Invoke();
	}
}
