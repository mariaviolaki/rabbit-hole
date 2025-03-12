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
		nameText.text = characterData == null ? "" : characterData.name;
		nameText.color = characterData == null ? gameOptions.DefaultTextColor : characterData.nameColor;
		nameText.font = characterData == null ? gameOptions.DefaultFont : characterData.nameFont;
	}

	void UpdateDialogueText(CharacterData characterData)
	{
		dialogueText.color = characterData == null ? gameOptions.DefaultTextColor : characterData.dialogueColor;
		dialogueText.font = characterData == null ? gameOptions.DefaultFont : characterData.dialogueFont;
	}
}
