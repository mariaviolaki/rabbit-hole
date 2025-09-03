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
		[SerializeField] string sceneName;
		[SerializeField] int nodeId;
		[SerializeField] string speakerText;
		[SerializeField] string dialogueText;
		[SerializeField] Color speakerColor;
		[SerializeField] Color dialogueColor;

		public string SceneName => sceneName;
		public int NodeId => nodeId;
		public string DialogueNodeId => TreeNodeUtilities.GetDialogueNodeId(sceneName, nodeId);
		public string SpeakerText => speakerText;
		public string DialogueText => dialogueText;
		public Color SpeakerColor => speakerColor;
		public Color DialogueColor => dialogueColor;

		public HistoryDialogueData(DialogueFlowController flowController, DialogueUI dialogueUI)
		{
			TextMeshProUGUI speakerTextData = dialogueUI.SpeakerText;
			TextMeshProUGUI dialogueTextData = dialogueUI.DialogueText;

			sceneName = flowController.CurrentSceneName;
			nodeId = flowController.CurrentNodeId;
			speakerText = speakerTextData.text;
			dialogueText = dialogueTextData.text;
			speakerColor = speakerTextData.color;
			dialogueColor = dialogueTextData.color;
		}

		public IEnumerator Load(DialogueManager dialogueManager, DialogueFlowController flowController)
		{
			dialogueManager.SetReadMode(DialogueReadMode.Forward);
			yield return flowController.JumpToScene(sceneName, nodeId);
		}
	}
}
