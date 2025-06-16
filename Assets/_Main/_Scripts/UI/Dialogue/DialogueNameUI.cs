using Characters;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueNameUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI nameText;

		public Coroutine ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText(characterData);
			return Show(isImmediate, fadeSpeed);
		}

		public Coroutine HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			return Hide(isImmediate, fadeSpeed);
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
	}
}
