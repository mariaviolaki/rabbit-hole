using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
	[SerializeField] GameObject nameContainer;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI dialogueText;

	public TextMeshProUGUI DialogueText { get { return dialogueText; } }

	public void ShowSpeaker(string speakerName)
	{
		nameContainer.SetActive(true);
		nameText.text = speakerName;
	}

	public void HideSpeaker()
	{
		nameContainer.SetActive(false);
	}
}
