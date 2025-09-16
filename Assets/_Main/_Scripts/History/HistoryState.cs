using Dialogue;
using System.Collections;
using UnityEngine;
using VN;

namespace History
{
	[System.Serializable]
	public class HistoryState
	{
		[SerializeField] HistoryDialogueData dialogueData;
		[SerializeField] HistoryAudioData audioData;
		[SerializeField] HistoryVisualData visualData;
		[SerializeField] HistoryCharacterData characterData;
		[SerializeField] HistoryVariableData variableData;

		public HistoryDialogueData Dialogue => dialogueData;
		public HistoryAudioData Audio => audioData;
		public HistoryVisualData Visuals => visualData;
		public HistoryCharacterData Characters => characterData;
		public HistoryVariableData Variables => variableData;

		public HistoryState(VNManager vnManager, DialogueManager dialogueManager)
		{
			audioData = new(vnManager.Game.Audio);
			variableData = new(vnManager.Game.Variables);
			dialogueData = new(dialogueManager.FlowController, vnManager.UI.Dialogue);
			visualData = new(dialogueManager.Visuals);
			characterData = new(dialogueManager.Characters);
		}

		// Resets progress to an older state
		public IEnumerator Load(VNManager vnManager, DialogueManager dialogueManager, VNOptionsSO vnOptions)
		{
			yield return visualData.Load(dialogueManager.Visuals, vnOptions);
			yield return audioData.Load(vnManager.Game.Audio, vnOptions);

			variableData.Load(vnManager.Game.Variables);
			characterData.Load(dialogueManager.Characters, vnOptions);

			yield return dialogueData.Load(dialogueManager, dialogueManager.FlowController);
		}
	}
}
