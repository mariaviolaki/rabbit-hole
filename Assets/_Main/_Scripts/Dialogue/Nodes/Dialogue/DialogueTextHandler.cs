using Game;
using History;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Variables;

namespace Dialogue
{
	public class DialogueTextHandler
	{
		const float MinAutoTime = 0.5f;
		const float MaxAutoTime = 100f;
		const float MinSkipTime = 0.001f;
		const float MaxSkipTime = 2f;
		const float BaseAutoTime = 1.2f;
		const float AutoTimePerCharacter = 1f;
		const float SkipSpeedMultiplier = 1f;

		readonly SettingsManager settingsManager;
		readonly GameProgressManager gameProgressManager;
		readonly DialogueManager dialogueManager;
		readonly VariableManager variableManager;
		readonly HistoryManager historyManager;
		readonly DialogueFlowController flowController;
		readonly VNOptionsSO vnOptions;
		readonly TextBuilder textBuilder;
		readonly DialogueContinuePromptUI continuePrompt;

		const TextTypeMode skippedTypeMode = TextTypeMode.Instant;
		readonly TextTypeMode typeMode;
		float readSpeed;
		bool isWaitingToAdvance;

		public void ForceComplete() => isWaitingToAdvance = false;

		public DialogueTextHandler(DialogueFlowController flowController)
		{
			this.flowController = flowController;
			this.dialogueManager = flowController.Dialogue;
			continuePrompt = dialogueManager.VN.UI.ContinuePrompt;

			variableManager = flowController.VN.Game.Variables;
			historyManager = flowController.VN.History;
			settingsManager = flowController.VN.Game.Settings;
			gameProgressManager = flowController.VN.Game.Progress;
			vnOptions = flowController.VN.Options;

			textBuilder = new(dialogueManager.VN.UI.Dialogue.DialogueText, vnOptions);
			typeMode = vnOptions.Dialogue.TextMode;

			dialogueManager.OnChangeReadMode += UpdateTextBuildMode;
		}

		public void Dispose()
		{
			dialogueManager.OnChangeReadMode -= UpdateTextBuildMode;
		}

		public void UpdateTextBuildMode(DialogueReadMode readMode)
		{
			if (readMode == DialogueReadMode.Skip)
				readSpeed = vnOptions.Dialogue.MaxTextSpeed;
			else
				readSpeed = settingsManager.TextSpeed;

			textBuilder.Speed = readSpeed;
		}

		public IEnumerator DisplayDialogue(List<DialogueTextSegment> textSegments)
		{
			for (int i = 0; i < textSegments.Count; i++)
			{
				DialogueTextSegment segment = textSegments[i];
				DialogueTextSegment nextSegment = (i == textSegments.Count - 1) ? null : textSegments[i + 1];

				yield return DisplayDialogueSegment(segment, nextSegment);
			}
		}

		IEnumerator DisplayDialogueSegment(DialogueTextSegment segment, DialogueTextSegment nextSegment)
		{
			string dialogueText = ScriptVariableUtilities.ParseText(segment.Text, variableManager);
			bool hasCapturedHistory = false;

			while (flowController.IsRunning)
			{
				float startTime = Time.time;

				// Wait for a specified duration before showing the text (unless forced to continue)
				if (segment != null && segment.IsAuto && !IsSkippableDialogue())
					while (flowController.IsRunning && isWaitingToAdvance && Time.time < startTime + segment.WaitTime) yield return null;

				continuePrompt.Hide();

				textBuilder.Speed = readSpeed;
				textBuilder.TypeMode = typeMode;
				if (IsSkippableDialogue())
				{
					textBuilder.Speed = vnOptions.Dialogue.MaxTextSpeed;
					textBuilder.TypeMode = skippedTypeMode;
				}

				if (segment.IsAppended)
					textBuilder.Append(dialogueText);
				else
					textBuilder.Write(dialogueText);

				isWaitingToAdvance = true;
				while (!CanContinueDialogue(nextSegment, dialogueText, startTime, textBuilder.IsBuilding, IsSkippableDialogue()))
				{
					ProcessCompletedText(ref hasCapturedHistory);
					yield return null;
				}

				ProcessCompletedText(ref hasCapturedHistory);

				isWaitingToAdvance = false;
				if (!textBuilder.IsBuilding) break;
			}
		}

		void ProcessCompletedText(ref bool hasCapturedHistory)
		{
			if (textBuilder.IsBuilding) return;

			if (!continuePrompt.IsVisible)
				continuePrompt.Show();

			if (!hasCapturedHistory)
			{
				historyManager.Capture();
				hasCapturedHistory = true;
			}
		}

		bool CanContinueDialogue(DialogueTextSegment nextSegment, string text, float startTime, bool isBuildingText, bool isSkippableLine)
		{
			if (!flowController.IsRunning || !isWaitingToAdvance) return true;

			bool canContinue = false;

			if (isSkippableLine)
				canContinue = !isBuildingText && Time.time >= startTime + GetSkipReadDelay();
			else if (nextSegment != null && nextSegment.IsAuto)
				canContinue = !isBuildingText;
			else if (dialogueManager.ReadMode == DialogueReadMode.Auto)
				canContinue = !isBuildingText && Time.time >= startTime + GetAutoReadDelay(text.Length);

			if (!canContinue && !isBuildingText && !continuePrompt.IsVisible)
				continuePrompt.Show();

			return canContinue;
		}

		bool IsSkippableDialogue()
		{
			if (!flowController.IsSkipping || settingsManager.SkipMode != DialogueSkipMode.Read)
				return flowController.IsSkipping;

			string dialogueNodeId = TreeNodeUtilities.GetDialogueNodeId(flowController.CurrentSceneName, flowController.CurrentNodeId);
			return gameProgressManager.HasReadLine(dialogueNodeId);
		}

		float GetAutoReadDelay(int textLength)
		{
			float speed = settingsManager.AutoSpeed;
			float delay = (BaseAutoTime + AutoTimePerCharacter * textLength) / speed;
			return Mathf.Clamp(delay, MinAutoTime, MaxAutoTime);
		}

		float GetSkipReadDelay()
		{
			float speed = vnOptions.Dialogue.SkipSpeed;
			float delay = (1 / speed) * SkipSpeedMultiplier;
			return Mathf.Clamp(delay, MinSkipTime, MaxSkipTime);
		}
	}
}
