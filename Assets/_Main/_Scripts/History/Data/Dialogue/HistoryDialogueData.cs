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

		public HistoryDialogueData(DialogueFlowController flowController)
		{
			sceneName = flowController.CurrentSceneName;
			nodeId = flowController.CurrentNodeId;

			if (flowController.CurrentNode is DialogueNode dialogueNode)
			{
				speakerText = dialogueNode.SpeakerData.Name;
				dialogueText = dialogueNode.FinalDialogueText;
				speakerColor = dialogueNode.SpeakerData.NameColor;
				dialogueColor = dialogueNode.SpeakerData.DialogueColor;
			}
		}

		public IEnumerator Load(DialogueManager dialogueManager, DialogueFlowController flowController)
		{
			dialogueManager.SetReadMode(DialogueReadMode.Forward);
			yield return flowController.JumpToScene(sceneName, nodeId);
		}
	}
}
