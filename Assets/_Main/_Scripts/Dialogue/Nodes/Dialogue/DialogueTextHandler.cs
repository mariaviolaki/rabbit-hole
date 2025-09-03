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
		const float BaseAutoTime = 0.8f;
		const float AutoTimePerCharacter = 0.8f;
		const float SkipSpeedMultiplier = 0.1f;

		readonly DialogueManager dialogueManager;
		readonly VariableManager variableManager;
		readonly HistoryManager historyManager;
		readonly DialogueFlowController flowController;
		readonly GameState gameState;
		readonly GameOptionsSO gameOptions;
		readonly TextBuilder textBuilder;
		readonly DialogueContinuePromptUI continuePrompt;

		TextBuildMode textMode;
		float readSpeed;
		bool isWaitingToAdvance;

		public void ForceComplete() => isWaitingToAdvance = false;

		public DialogueTextHandler(GameManager gameManager, DialogueManager dialogueManager, DialogueFlowController flowController)
		{
			this.dialogueManager = dialogueManager;
			this.flowController = flowController;
			continuePrompt = dialogueManager.UI.ContinuePrompt;

			variableManager = gameManager.Variables;
			historyManager = gameManager.History;
			gameState = gameManager.StateManager.State;
			gameOptions = gameManager.Options;

			textBuilder = new(dialogueManager.UI.Dialogue.DialogueText);

			dialogueManager.OnChangeReadMode += UpdateTextBuildMode;
		}

		public void Dispose()
		{
			dialogueManager.OnChangeReadMode -= UpdateTextBuildMode;
		}

		public void UpdateTextBuildMode(DialogueReadMode readMode)
		{
			if (readMode == DialogueReadMode.Skip)
				readSpeed = TextBuilder.MaxSpeed;
			else
				readSpeed = gameState.TextSpeed;

			textBuilder.Speed = readSpeed;
		}

		public IEnumerator DisplayDialogue(List<DialogueTextSegment> textSegments)
		{
			bool isSkippableDialogue = IsSkippableDialogue();

			for (int i = 0; i < textSegments.Count; i++)
			{
				DialogueTextSegment segment = textSegments[i];
				DialogueTextSegment nextSegment = (i == textSegments.Count - 1) ? null : textSegments[i + 1];

				yield return DisplayDialogueSegment(segment, nextSegment, isSkippableDialogue);
			}
		}

		IEnumerator DisplayDialogueSegment(DialogueTextSegment segment, DialogueTextSegment nextSegment, bool isSkippableLine)
		{
			string dialogueText = ScriptVariableUtilities.ParseText(segment.Text, variableManager);
			bool hasNewHistory = nextSegment == null;
			bool hasCapturedHistory = false;

			while (flowController.IsRunning)
			{
				float startTime = Time.time;

				// Wait for a specified duration before showing the text (unless forced to continue)
				if (segment != null && segment.IsAuto && !isSkippableLine)
					while (flowController.IsRunning && isWaitingToAdvance && Time.time < startTime + segment.WaitTime) yield return null;

				continuePrompt.Hide();
				textBuilder.Speed = readSpeed;

				if (segment.IsAppended)
					textBuilder.Append(dialogueText, textMode);
				else
					textBuilder.Write(dialogueText, textMode);

				isWaitingToAdvance = true;
				while (!CanContinueDialogue(nextSegment, dialogueText, startTime, textBuilder.IsBuilding, isSkippableLine))
				{
					ProcessCompletedText(hasNewHistory, ref hasCapturedHistory);
					yield return null;
				}

				ProcessCompletedText(hasNewHistory, ref hasCapturedHistory);

				isWaitingToAdvance = false;
				if (!textBuilder.IsBuilding) break;
			}
		}

		void ProcessCompletedText(bool hasNewHistory, ref bool hasCapturedHistory)
		{
			if (textBuilder.IsBuilding) return;

			if (!continuePrompt.IsVisible)
				continuePrompt.Show();

			if (hasNewHistory && !hasCapturedHistory)
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
			if (!flowController.IsSkipping || gameState.SkipMode != DialogueSkipMode.Read)
				return flowController.IsSkipping;

			string dialogueNodeId = TreeNodeUtilities.GetDialogueNodeId(flowController.CurrentSceneName, flowController.CurrentNodeId);
			return gameState.HasReadLine(dialogueNodeId);
		}

		float GetAutoReadDelay(int textLength)
		{
			float speed = gameState.AutoSpeed;
			float delay = (BaseAutoTime + AutoTimePerCharacter * textLength) / speed;
			return Mathf.Clamp(delay, MinAutoTime, MaxAutoTime);
		}

		float GetSkipReadDelay()
		{
			float speed = gameOptions.Dialogue.SkipSpeed;
			float delay = (1 / speed) * SkipSpeedMultiplier;
			return Mathf.Clamp(delay, MinSkipTime, MaxSkipTime);
		}
	}
}
