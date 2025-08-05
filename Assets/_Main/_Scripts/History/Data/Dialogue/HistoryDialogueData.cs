using Dialogue;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;

namespace History
{
	[System.Serializable]
	public class HistoryDialogueData
	{
		[SerializeField] string sectionName;
		[SerializeField] int nodeId;
		[SerializeField] string speakerText;
		[SerializeField] string dialogueText;
		[SerializeField] Color speakerColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] string speakerFont;
		[SerializeField] string dialogueFont;

		public string SectionName => sectionName;
		public int NodeId => nodeId;
		public string DialogueNodeId => TreeNodeUtilities.GetDialogueNodeId(sectionName, nodeId);
		public string SpeakerText => speakerText;
		public string DialogueText => dialogueText;
		public Color SpeakerColor => speakerColor;
		public Color DialogueColor => dialogueColor;
		public string SpeakerFont => speakerFont;
		public string DialogueFont => dialogueFont;

		public HistoryDialogueData(DialogueFlowController flowController, DialogueUI dialogueUI)
		{
			TextMeshProUGUI speakerTextData = dialogueUI.SpeakerText;
			TextMeshProUGUI dialogueTextData = dialogueUI.DialogueText;

			sectionName = flowController.CurrentSectionName;
			nodeId = flowController.CurrentNodeId;
			speakerText = speakerTextData.text;
			dialogueText = dialogueTextData.text;
			speakerColor = speakerTextData.color;
			dialogueColor = dialogueTextData.color;
			speakerFont = speakerTextData.font.name;
			dialogueFont = dialogueTextData.font.name;
		}

		public IEnumerator Load(DialogueManager dialogueManager, DialogueFlowController flowController)
		{
			dialogueManager.SetReadMode(DialogueReadMode.Forward);
			yield return flowController.JumpToSection(sectionName, nodeId);
		}
	}
}
