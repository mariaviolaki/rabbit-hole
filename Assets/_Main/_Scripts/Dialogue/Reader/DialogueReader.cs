using Characters;
using Commands;
using History;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueReader
	{
		const float MinAutoTime = 0.5f;
		const float MaxAutoTime = 100f;
		const float MinSkipTime = 0.001f;
		const float MaxSkipTime = 2f;
		const float BaseAutoTime = 0.8f;
		const float AutoTimePerCharacter = 0.8f;
		const float SkipSpeedMultiplier = 0.1f;

		readonly DialogueSystem dialogueSystem;
		readonly GameOptionsSO gameOptions;
		readonly TextBuilder textBuilder;
		readonly CharacterManager characterManager;
		readonly CommandManager commandManager;
		readonly VisualNovelUI visualNovelUI;
		readonly DialogueContinuePromptUI continuePrompt;
		readonly LogicSegmentManager logicSegmentManager;
		readonly DialogueStack dialogueStack;
		readonly ScriptValueParser valueParser;
		readonly InputManagerSO inputManager;
		readonly HistoryManager historyManager;

		TextBuilder.TextMode textMode;
		float readSpeed;
		bool isReading = false;
		bool isRunning = false;
		bool isWaitingToAdvance = true;

		public bool IsWaitingToAdvance => isWaitingToAdvance;
		public bool IsRunning => isRunning;
		public bool IsReading => isRunning && isReading;

		public DialogueStack Stack => dialogueStack;
		public ScriptValueParser ValueParser => valueParser;

		public void StartDialogue() => isRunning = true;
		public void StopDialogue() => isRunning = false;

		public void ContinueDialogue() => isReading = true;
		public void PauseDialogue() => isReading = false;

		public void SendAdvanceInput() => isWaitingToAdvance = false;
		public void WaitForInput() => isWaitingToAdvance = true;

		public DialogueReader(DialogueSystem dialogueSystem)
		{
			this.dialogueSystem = dialogueSystem;
			gameOptions = dialogueSystem.Options;
			characterManager = dialogueSystem.Characters;
			commandManager = dialogueSystem.Commands;
			visualNovelUI = dialogueSystem.UI;
			continuePrompt = dialogueSystem.ContinuePrompt;
			inputManager = dialogueSystem.InputManager;
			historyManager = dialogueSystem.History;
			logicSegmentManager = dialogueSystem.Logic;

			textBuilder = new(visualNovelUI.Dialogue.DialogueText);
			dialogueStack = new();
			valueParser = new(dialogueSystem);

			textMode = gameOptions.Dialogue.TextMode;

			inputManager.OnAdvance += SendAdvanceInput;
		}

		public void Dispose()
		{
			inputManager.OnAdvance -= SendAdvanceInput;
		}

		public void UpdateReadMode(DialogueReadMode readMode)
		{
			if (readMode == DialogueReadMode.Skip)
				readSpeed = textBuilder.MaxSpeed;
			else
				readSpeed = gameOptions.Dialogue.TextSpeed;

			textBuilder.Speed = readSpeed;

			if (readMode == DialogueReadMode.Auto || readMode == DialogueReadMode.Skip)
				SendAdvanceInput();
		}

		public IEnumerator ReadImmediate(string dialogueText)
		{
			isReading = false;

			while (textBuilder.IsBuilding) yield return null;
			isReading = true;

			textBuilder.Speed = textBuilder.MaxSpeed;
			textBuilder.Write(dialogueText, textMode);

			while (textBuilder.IsBuilding) yield return null;

			continuePrompt.Show();
			isReading = false;
		}

		// Read lines directly from dialogue files (each line includes: speaker, dialogue, commands)
		public IEnumerator Read(DialogueReadMode dialogueReadMode)
		{
			UpdateReadMode(dialogueReadMode);
			isReading = false;

			while (isRunning)
			{
				while (isReading) yield return null;
				isReading = true;

				// Get the next non-null line and remove any blocks that are complete
				string rawLine = dialogueStack.GetCurrentLine();
				
				while (rawLine == null)
				{
					// Don't end the dialogue if there are no lines left - check if the stack was refreshed
					yield return WaitForStackUpdate();
					
					dialogueStack.Proceed(dialogueStack.GetBlock());
					rawLine = dialogueStack.GetCurrentLine();
				}

				// Cache the block this line belongs to because it might change during this iteration
				DialogueBlock dialogueBlock = dialogueStack.GetBlock();

				// Wait for any previous skipped transitions to complete smoothly
				while (!commandManager.IsIdle()) yield return null;

				DialogueLine dialogueLine = DialogueParser.Parse(rawLine, logicSegmentManager);
				if (dialogueLine.Logic == null)
					yield return ProcessDialogueLine(dialogueLine);
				else
					yield return ProcessLogicSegment(dialogueLine);

				yield return ProcessCompletedLine(dialogueBlock, dialogueLine);
				isReading = false;
			}
		}

		IEnumerator ProcessCompletedLine(DialogueBlock dialogueBlock, DialogueLine dialogueLine)
		{
			bool isHistoryUpdated = false;

			// Don't progress to new lines while the user is viewing history
			yield return ProcessHistory(isUpdated => isHistoryUpdated = isUpdated);
			if (isHistoryUpdated) yield break;

			// If the player didn't view history and there was dialogue in this line, capture the latest history state
			if (dialogueLine.Dialogue != null)
				historyManager.Capture();

			dialogueStack.Proceed(dialogueBlock);
		}

		IEnumerator WaitForStackUpdate()
		{
			bool isHistoryUpdated = false;

			while (!isHistoryUpdated)
				yield return ProcessHistory(isUpdated => isHistoryUpdated = isUpdated);
		}

		IEnumerator ProcessHistory(Action<bool> isUpdated)
		{
			bool isHistoryUpdated = false;

			if (inputManager.IsLogPanelOpen)
				yield return ProcessLogPanelHistory(isUpdated => isHistoryUpdated = isUpdated);
			else if (historyManager.HistoryNode != null)
				yield return ProcessNavigationHistory(isUpdated => isHistoryUpdated = isUpdated);

			isUpdated(isHistoryUpdated);
		}

		IEnumerator ProcessNavigationHistory(Action<bool> isUpdated)
		{
			// The player is navigating back to older states
			while (isRunning && isWaitingToAdvance && !inputManager.IsLogPanelOpen) yield return null;

			if (inputManager.IsLogPanelOpen)
			{
				bool isLogHistoryApplied = false;
				yield return ProcessLogPanelHistory(isApplied => isLogHistoryApplied = isApplied);

				isUpdated(isLogHistoryApplied);
			}
			else
			{
				// When the player goes forward again, apply the latest history state
				bool isNavigationHistoryApplied = false;
				yield return historyManager.ApplyHistory(isApplied => isNavigationHistoryApplied = isApplied);

				if (isNavigationHistoryApplied)
				{
					UpdateReadMode(DialogueReadMode.Forward);
					DialogueBlock lastBlock = dialogueSystem.Reader.Stack.GetBlock();
					dialogueSystem.Reader.Stack.Proceed(lastBlock);
				}

				isUpdated(isNavigationHistoryApplied);
			}
		}

		IEnumerator ProcessLogPanelHistory(Action<bool> isUpdated)
		{
			while (inputManager.IsLogPanelOpen) yield return null;

			if (historyManager.LogNode == null)
			{
				isUpdated(false);
				yield break;
			}

			// If the player chose to rewind instead of closing the panel, apply the history
			bool isHistoryApplied = false;
			yield return historyManager.ApplyHistory(isApplied => isHistoryApplied = isApplied);

			if (isHistoryApplied)
			{
				UpdateReadMode(DialogueReadMode.Forward);
			}

			isUpdated(isHistoryApplied);
		}

		IEnumerator ProcessDialogueLine(DialogueLine line)
		{
			if (line.Dialogue != null)
			{
				SetSpeaker(line.Speaker);
				yield return DisplayDialogue(line);
			}

			if (line.Commands != null)
				yield return RunCommands(line.Commands.CommandList);
		}

		IEnumerator ProcessLogicSegment(DialogueLine line)
		{
			logicSegmentManager.Add(line.Logic);

			// When execution is finished, the stack will have been updated
			if (logicSegmentManager.HasPendingLogic)
				yield return logicSegmentManager.WaitForExecution();
		}

		IEnumerator DisplayDialogue(DialogueLine line)
		{
			List<DialogueTextData.Segment> lineSegments = line.Dialogue.Segments;

			for (int i = 0; i < lineSegments.Count; i++)
			{
				DialogueTextData.Segment segment = lineSegments[i];
				DialogueTextData.Segment nextSegment = (i == lineSegments.Count - 1) ? null : lineSegments[i + 1];

				yield return DisplayDialogueSegment(segment, nextSegment);
			}
		}

		IEnumerator DisplayDialogueSegment(DialogueTextData.Segment segment, DialogueTextData.Segment nextSegment)
		{
			string dialogueText = valueParser.ParseText(segment.Text);

			while (isRunning)
			{
				float startTime = Time.time;

				// Wait for a specified duration before showing the text (unless forced to continue)
				if (segment != null && segment.IsAuto && dialogueSystem.ReadMode != DialogueReadMode.Skip)
					while (isRunning && isWaitingToAdvance && Time.time < startTime + segment.WaitTime) yield return null;

				continuePrompt.Hide();
				textBuilder.Speed = readSpeed;

				if (segment.IsAppended)
					textBuilder.Append(dialogueText, textMode);
				else
					textBuilder.Write(dialogueText, textMode);

				isWaitingToAdvance = true;
				while (!CanContinueDialogue(nextSegment, dialogueText, startTime, textBuilder.IsBuilding)) yield return null;

				continuePrompt.Show();

				if (!textBuilder.IsBuilding) break;
			}
		}

		public IEnumerator RunCommands(List<DialogueCommandData.Command> commandList)
		{
			List<CommandProcess> processesToWait = new List<CommandProcess>();

			foreach (DialogueCommandData.Command command in commandList)
			{
				CommandProcess process = commandManager.Execute(command.Name, command.Arguments);
				if (process == null) continue;

				if (process.IsBlocking || command.IsWaiting || command.Name.ToLower() == "wait")
					processesToWait.Add(process);
			}

			// Wait to execute all processes of this line concurrently
			if (processesToWait.Count > 0)
			{
				isWaitingToAdvance = true;
				while (true)
				{
					// Stop when all processes end, or the user clicks to skip them
					if (processesToWait.All(p => p.IsCompleted)) break;
					else if (!isRunning || !isWaitingToAdvance)
					{
						commandManager.SkipCommands();
						break;
					}
					yield return null;
				}
			}
		}

		public void SetSpeaker(DialogueSpeakerData speakerData)
		{
			if (speakerData == null || string.IsNullOrEmpty(speakerData.Name))
			{
				visualNovelUI.Dialogue.HideSpeaker();
				return;
			}

			Character character = characterManager.GetCharacter(speakerData.Name);
			speakerData.DisplayName = valueParser.ParseText(character.Data.Name);

			ChangeSpeakerDisplayName(character, speakerData.DisplayName);
			ChangeSpeakerPosition(character, speakerData.XPos, speakerData.YPos);
			ChangeSpeakerGraphics(character, speakerData.Layers);
			SetSpeakerName(character.Data);
		}

		public void SetSpeakerName(CharacterData characterData)
		{
			visualNovelUI.Dialogue.ShowSpeaker(characterData);
		}

		void ChangeSpeakerDisplayName(Character character, string displayName)
		{
			if (string.IsNullOrEmpty(displayName)) return;

			character.SetDisplayName(displayName);
		}

		void ChangeSpeakerPosition(Character character, float xPos, float yPos)
		{
			if (character is not GraphicsCharacter || (float.IsNaN(xPos) && float.IsNaN(yPos))) return;

			((GraphicsCharacter)character).SetPosition(xPos, yPos, false);
		}

		void ChangeSpeakerGraphics(Character character, Dictionary<SpriteLayerType, string> graphics)
		{
			if (graphics == null) return;

			if (character is SpriteCharacter)
			{
				foreach (var layer in graphics)
					((SpriteCharacter)character).SetSprite(layer.Value, layer.Key);
			}
			else if (character is Model3DCharacter)
			{
				foreach (string expressionName in graphics.Values)
					((Model3DCharacter)character).SetExpression(expressionName);
			}
		}

		bool CanContinueDialogue(DialogueTextData.Segment nextSegment, string text, float startTime, bool isBuildingText)
		{
			if (!isRunning || !isWaitingToAdvance || historyManager.IsViewingHistory) return true;

			bool canContinue = false;

			if (nextSegment != null && nextSegment.IsAuto)
				canContinue = !isBuildingText;
			else if (dialogueSystem.ReadMode == DialogueReadMode.Forward)
				canContinue = false;
			if (dialogueSystem.ReadMode == DialogueReadMode.Auto)
				canContinue = !isBuildingText && Time.time >= startTime + GetAutoReadDelay(text.Length);
			else if (dialogueSystem.ReadMode == DialogueReadMode.Skip)
				canContinue = !isBuildingText && Time.time >= startTime + GetSkipReadDelay();

			if (!canContinue && !isBuildingText && !continuePrompt.IsVisible)
				continuePrompt.Show();

			return canContinue;
		}

		float GetAutoReadDelay(int textLength)
		{
			float speed = gameOptions.Dialogue.AutoDialogueSpeed;
			float delay = (BaseAutoTime + AutoTimePerCharacter * textLength) / speed;
			return Mathf.Clamp(delay, MinAutoTime, MaxAutoTime);
		}

		float GetSkipReadDelay()
		{
			float speed = gameOptions.Dialogue.AutoDialogueSpeed;
			float delay = (1 / speed) * SkipSpeedMultiplier;
			return Mathf.Clamp(delay, MinSkipTime, MaxSkipTime);
		}
	}
}
