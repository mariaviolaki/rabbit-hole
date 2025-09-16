using System.Collections;
using TMPro;
using UnityEngine;
using VN;

namespace Dialogue
{
	public class TextBuilder
	{
		enum BuildMode { Write, Append }

		const float MinWaitTime = 0.02f;
		const float InstantSpeedMultiplier = 5f;
		const float TypeSpeedMultiplier = 0.21f;
		const float FadeSpeedMultiplier = 0.095f;

		readonly VNOptionsSO vnOptions;
		readonly TMP_Text tmpText;

		Coroutine buildProcess;
		TextTypeMode typeMode;
		float speed;

		public bool IsBuilding => buildProcess != null;
		public float Speed
		{
			get { return speed; }
			set { speed = Mathf.Clamp(value, vnOptions.Dialogue.MinTextSpeed, vnOptions.Dialogue.MaxTextSpeed); }
		}
		public TextTypeMode TypeMode { get { return typeMode; } set { typeMode = value; } }

		public TextBuilder(TMP_Text tmpText, VNOptionsSO vnOptions)
		{
			this.tmpText = tmpText;
			this.tmpText.text = "";
			this.vnOptions = vnOptions;

			speed = vnOptions.Dialogue.MaxTextSpeed / 2;
		}

		public bool Write(string newText)
		{
			return StartProcess(newText, BuildMode.Write);
		}

		public bool Append(string newText)
		{
			return StartProcess(newText, BuildMode.Append);
		}

		public bool Stop()
		{
			// There was no active building process
			if (buildProcess == null) return false;

			CompleteProcess();
			return true;
		}

		bool StartProcess(string newText, BuildMode buildType)
		{
			bool isProcessInterrupted = Stop();

			typeMode = isProcessInterrupted ? TextTypeMode.Instant : typeMode;
			string oldText = tmpText.text;
			string preText = buildType == BuildMode.Append || isProcessInterrupted ? oldText : "";
			int preTextLength = BuildPreText(preText);

			if (isProcessInterrupted) return false;

			if (typeMode == TextTypeMode.Instant || typeMode == TextTypeMode.InstantFade)
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

			if (typeMode == TextTypeMode.InstantFade)
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
			if (typeMode == TextTypeMode.TypedFade)
				tmpText.maxVisibleCharacters = fullTextLength;

			float rawTimePerCharacter = (1f / speed) * TypeSpeedMultiplier;
			float timePerCharacter = Mathf.Max(MinWaitTime, rawTimePerCharacter);
			int charactersPerIteration = GetCharactersPerIteration(newTextLength, rawTimePerCharacter);
			float maxTime = timePerCharacter * Mathf.CeilToInt((float)newTextLength / charactersPerIteration);

			if (typeMode == TextTypeMode.Typed)
				buildProcess = tmpText.StartCoroutine(StartTyping(maxTime, timePerCharacter, newTextLength, charactersPerIteration));
			else if (typeMode == TextTypeMode.TypedFade)
				buildProcess = tmpText.StartCoroutine(StartFadingIn(maxTime, preTextLength, newTextLength, charactersPerIteration));
		}

		IEnumerator StartInstantFadingIn(int preTextLength, int newTextLength)
		{
			HideText(preTextLength);

			float[] transparencyValues = new float[newTextLength];
			float fadeTime = 1 / speed * InstantSpeedMultiplier;
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
				if (fadeTimeElapsed >= MinWaitTime)
				{
					fadeTimeElapsed -= MinWaitTime;
					float transparencyStep = FadeSpeedMultiplier * 255;
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
			if (waitTime < MinWaitTime)
			{
				float waitRatio = MinWaitTime / waitTime;
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
