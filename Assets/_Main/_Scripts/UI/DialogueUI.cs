using Characters;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] GameObject nameContainer;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI dialogueText;

	public TextMeshProUGUI DialogueText { get { return dialogueText; } }

	public void ShowSpeaker(CharacterData characterData)
	{
		UpdateNameText(characterData);
		UpdateDialogueText(characterData);
		nameContainer.SetActive(true);
	}

	public void HideSpeaker()
	{
		UpdateDialogueText(null);
		nameContainer.SetActive(false);
	}

	void UpdateNameText(CharacterData characterData)
	{
		nameText.text = characterData == null ? "" : characterData.DisplayName;
		nameText.color = characterData == null ? gameOptions.Dialogue.DefaultTextColor : characterData.NameColor;
		nameText.font = characterData == null ? gameOptions.Dialogue.DefaultFont : characterData.NameFont;
	}

	void UpdateDialogueText(CharacterData characterData)
	{
		dialogueText.color = characterData == null ? gameOptions.Dialogue.DefaultTextColor : characterData.DialogueColor;
		dialogueText.font = characterData == null ? gameOptions.Dialogue.DefaultFont : characterData.DialogueFont;
	}
}
