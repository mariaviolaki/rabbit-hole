using Characters;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueNameUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI nameText;

		public Coroutine ShowSpeaker(CharacterData characterData, float fadeSpeed = 0)
		{
			UpdateNameText(characterData);

			return Show(fadeSpeed);
		}

		public Coroutine HideSpeaker(float fadeSpeed = 0)
		{
			return Hide(fadeSpeed);
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
