using System.Collections;
using TMPro;
using UnityEngine;

namespace Dialogue
{
	public class TextBuilder
	{
		public enum TextMode { Instant, Typed, InstantFade, TypedFade }
		enum BuildMode { Write, Append }

		const float maxSpeed = 15f;
		const float minWaitTime = 0.02f;
		const float instantSpeedMultiplier = 5f;
		const float typeSpeedMultiplier = 0.1f;
		const float fadeSpeedMultiplier = 0.095f;

		Coroutine buildProcess;

		// Configurable from outside
		TMP_Text tmpText;
		TextMode textType;
		float speed;

		public bool IsBuilding { get { return buildProcess != null; } }
		public float MaxSpeed { get { return maxSpeed; } }
		public float Speed { get { return speed; } set { speed = Mathf.Clamp(value, 1f, MaxSpeed); } }

		public TextBuilder(TMP_Text tmpText)
		{
			this.tmpText = tmpText;
			this.tmpText.text = "";
			speed = MaxSpeed / 2;
		}

		public bool Write(string newText, TextMode textType)
		{
			return StartProcess(newText, BuildMode.Write, textType);
		}

		public bool Append(string newText, TextMode textType)
		{
			return StartProcess(newText, BuildMode.Append, textType);
		}

		public bool Stop()
		{
			// There was no active building process
			if (buildProcess == null) return false;

			CompleteProcess();
			return true;
		}

		bool StartProcess(string newText, BuildMode buildType, TextMode textType)
		{
			bool isProcessInterrupted = Stop();

			this.textType = isProcessInterrupted ? TextMode.Instant : textType;
			string oldText = tmpText.text;
			string preText = buildType == BuildMode.Append || isProcessInterrupted ? oldText : "";
			int preTextLength = BuildPreText(preText);

			if (isProcessInterrupted) return false;

			if (textType == TextMode.Instant || textType == TextMode.InstantFade)
				BuildInstantText(newText, preTextLength);
			else
				BuildTypedText(newText, preTextLength);

			return true;
		}

		int BuildPreText(string preText)
		{
			tmpText.text = preText;
			tmpText.ForceMeshUpdate();

			int preTextLength = tmpText.textInfo.characterCount;
			tmpText.maxVisibleCharacters = preTextLength;

			return preTextLength;
		}

		void BuildInstantText(string newText, int preTextLength)
		{
			tmpText.text += newText;

			tmpText.ForceMeshUpdate();
			int fullTextLenth = tmpText.textInfo.characterCount;
			tmpText.maxVisibleCharacters = fullTextLenth;

			if (textType == TextMode.InstantFade)
			{
				int newTextLength = fullTextLenth - preTextLength;
				buildProcess = tmpText.StartCoroutine(StartInstantFadingIn(preTextLength, newTextLength));
			}
		}

		void BuildTypedText(string newText, int preTextLength)
		{
			tmpText.text += newText;
			tmpText.ForceMeshUpdate();

			int fullTextLength = tmpText.textInfo.characterCount;
			int newTextLength = fullTextLength - preTextLength;
			if (textType == TextMode.TypedFade)
				tmpText.maxVisibleCharacters = fullTextLength;

			float rawTimePerCharacter = 1f / speed * typeSpeedMultiplier;
			float timePerCharacter = Mathf.Max(minWaitTime, rawTimePerCharacter);
			int charactersPerIteration = GetCharactersPerIteration(newTextLength, rawTimePerCharacter);
			float maxTime = timePerCharacter * Mathf.CeilToInt((float)newTextLength / charactersPerIteration);

			if (textType == TextMode.Typed)
				buildProcess = tmpText.StartCoroutine(StartTyping(maxTime, timePerCharacter, newTextLength, charactersPerIteration));
			else if (textType == TextMode.TypedFade)
				buildProcess = tmpText.StartCoroutine(StartFadingIn(maxTime, preTextLength, newTextLength, charactersPerIteration));
		}

		IEnumerator StartInstantFadingIn(int preTextLength, int newTextLength)
		{
			HideText(preTextLength);

			float[] transparencyValues = new float[newTextLength];
			float fadeTime = 1 / speed * instantSpeedMultiplier;
			float newestAlpha = 0;

			while (newestAlpha < 255)
			{
				float frameTimeRatio = Time.deltaTime / fadeTime;
				float transparencyStep = frameTimeRatio * 255;
				newestAlpha = FadeInText(transparencyValues, 0, newTextLength - 1, preTextLength, transparencyStep);

				yield return null;
			}

			buildProcess = null;
		}

		IEnumerator StartTyping(float maxTime, float timePerCharacter, int newTextLength, int charactersPerIteration)
		{
			float timeElapsed = 0;
			float charTimeElapsed = 0;

			while (timeElapsed < maxTime || tmpText.maxVisibleCharacters < newTextLength)
			{
				timeElapsed += Time.deltaTime;
				charTimeElapsed += Time.deltaTime;
				if (charTimeElapsed >= timePerCharacter)
				{
					charTimeElapsed -= timePerCharacter;
					tmpText.maxVisibleCharacters += charactersPerIteration;
					tmpText.ForceMeshUpdate();
				}

				yield return null;
			}

			buildProcess = null;
		}

		IEnumerator StartFadingIn(float maxTime, int preTextLength, int newTextLength, int charactersPerIteration)
		{
			HideText(preTextLength);

			float[] transparencyValues = new float[newTextLength];
			float timePerCharacter = maxTime / newTextLength;

			int minRange = 0;
			int maxRange = charactersPerIteration;
			float newestAlpha = 0;

			float charTimeElapsed = 0;
			float fadeTimeElapsed = 0;

			while (maxRange < transparencyValues.Length - 1 || newestAlpha < 255)
			{
				charTimeElapsed += Time.deltaTime;
				fadeTimeElapsed += Time.deltaTime;

				// Increase max range
				if (charTimeElapsed >= timePerCharacter)
				{
					charTimeElapsed -= timePerCharacter;
					maxRange = Mathf.Min(maxRange + charactersPerIteration, newTextLength - 1);
				}

				// Increase min range
				if (transparencyValues[minRange] >= 255)
				{
					minRange = Mathf.Min(minRange + charactersPerIteration, newTextLength - 1);
				}

				// Fade in all the characters in the current range
				if (fadeTimeElapsed >= minWaitTime)
				{
					fadeTimeElapsed -= minWaitTime;
					float transparencyStep = fadeSpeedMultiplier * 255;
					newestAlpha = FadeInText(transparencyValues, minRange, maxRange, preTextLength, transparencyStep);
				}

				yield return null;
			}

			buildProcess = null;
		}

		void CompleteProcess()
		{
			tmpText.StopCoroutine(buildProcess);
			buildProcess = null;
		}

		void HideText(int preTextLength)
		{
			tmpText.ForceMeshUpdate();

			// Set all the new text characters to invisible
			for (int i = 0; i < tmpText.textInfo.characterCount; i++)
			{
				int newAlpha = i < preTextLength ? 255 : 0;
				ChangeCharacterTransparency(i, newAlpha);
			}

			tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}

		float FadeInText(float[] transparencyValues, int minRange, int maxRange, int preTextLength, float transparencyStep)
		{
			float newestAlpha = 0;

			// Equally fade in all the characters in the given range by a given amount
			for (int i = minRange; i <= maxRange; i++)
			{
				newestAlpha = Mathf.MoveTowards(transparencyValues[i], 255f, transparencyStep);
				transparencyValues[i] = newestAlpha;
				ChangeCharacterTransparency(preTextLength + i, newestAlpha);
			}

			tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
			return newestAlpha;
		}

		int GetCharactersPerIteration(int textCount, float waitTime)
		{
			// Reveal more characters when the wait time is less than minimum
			if (waitTime < minWaitTime)
			{
				float waitRatio = minWaitTime / waitTime;
				int characterCount = Mathf.RoundToInt((waitRatio - 1f) * 5f) + 1;

				return characterCount;
			}

			return 1;
		}

		void ChangeCharacterTransparency(int charIndex, float alpha)
		{
			TMP_TextInfo textInfo = tmpText.textInfo;

			// Get information for the character at the current index
			TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
			if (!charInfo.isVisible) return;

			// Get all vertex colors for this text using this character's material
			int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
			Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

			// Change the transparency for all 4 vertices of this character
			vertexColors[charInfo.vertexIndex + 0].a = (byte)alpha;
			vertexColors[charInfo.vertexIndex + 1].a = (byte)alpha;
			vertexColors[charInfo.vertexIndex + 2].a = (byte)alpha;
			vertexColors[charInfo.vertexIndex + 3].a = (byte)alpha;
		}
	}
}
