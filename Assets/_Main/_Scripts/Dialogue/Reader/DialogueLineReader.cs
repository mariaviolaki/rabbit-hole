using Characters;
using Commands;
using History;
using Logic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueLineReader
	{
		const float MinAutoTime = 0.5f;
		const float MaxAutoTime = 100f;
		const float MinSkipTime = 0.001f;
		const float MaxSkipTime = 2f;
		const float BaseAutoTime = 0.8f;
		const float AutoTimePerCharacter = 0.8f;
		const float SkipSpeedMultiplier = 0.1f;

		readonly DialogueManager dialogueManager;
		readonly DialogueReader dialogueReader;
		readonly GameOptionsSO gameOptions;
		readonly CharacterManager characterManager;
		readonly CommandManager commandManager;
		readonly VisualNovelUI visualNovelUI;
		readonly HistoryManager historyManager;
		readonly TextBuilder textBuilder;
		readonly ScriptValueParser valueParser;
		readonly GameState gameState;
		
		TextBuildMode textMode;
		float readSpeed;

		public ScriptValueParser ValueParser => valueParser;
		public bool IsBuildingText => textBuilder.IsBuilding;

		public DialogueLineReader(DialogueManager dialogueManager, DialogueReader dialogueReader)
		{
			this.dialogueManager = dialogueManager;
			this.dialogueReader = dialogueReader;
			gameOptions = dialogueManager.Options;
			characterManager = dialogueManager.Characters;
			commandManager = dialogueManager.Commands;
			visualNovelUI = dialogueManager.UI;
			historyManager = dialogueManager.History;
			gameState = dialogueManager.State;

			textBuilder = new(visualNovelUI.Dialogue.DialogueText);
			valueParser = new(dialogueManager.VariableManager, dialogueManager.TagBank);

			textMode = gameOptions.Dialogue.TextMode;
		}

		public void ReadImmediateText(string dialogueText)
		{
			textBuilder.Speed = TextBuilder.MaxSpeed;
			textBuilder.Write(dialogueText, textMode);
		}

		public void UpdateTextBuildMode(DialogueReadMode readMode)
		{
			if (readMode == DialogueReadMode.Skip)
				readSpeed = TextBuilder.MaxSpeed;
			else
				readSpeed = gameState.TextSpeed;

			textBuilder.Speed = readSpeed;
		}

		public IEnumerator ProcessDialogueLine(DialogueLine line)
		{
			// If this is a line where logic is executed, there should be no dialogue
			if (line.Logic != null) yield break;

			bool isSkippableLine = IsSkippableLine(line.FilePath, line.LineNumber);

			if (line.Commands != null)
				yield return RunCommands(line.Commands.CommandList, isSkippableLine);

			if (line.Dialogue != null)
			{
				SetSpeaker(line.Speaker);
				yield return DisplayDialogue(line, isSkippableLine);
			}
		}

		IEnumerator DisplayDialogue(DialogueLine line, bool isSkippableLine)
		{
			List<DialogueTextData.Segment> lineSegments = line.Dialogue.Segments;

			for (int i = 0; i < lineSegments.Count; i++)
			{
				DialogueTextData.Segment segment = lineSegments[i];
				DialogueTextData.Segment nextSegment = (i == lineSegments.Count - 1) ? null : lineSegments[i + 1];

				yield return DisplayDialogueSegment(segment, nextSegment, isSkippableLine);
			}
		}

		IEnumerator DisplayDialogueSegment(DialogueTextData.Segment segment, DialogueTextData.Segment nextSegment, bool isSkippableLine)
		{
			string dialogueText = valueParser.ParseText(segment.Text);
			bool hasNewHistory = nextSegment == null;
			bool hasCapturedHistory = false;

			while (dialogueReader.IsRunning)
			{
				float startTime = Time.time;

				// Wait for a specified duration before showing the text (unless forced to continue)
				if (segment != null && segment.IsAuto && !isSkippableLine)
					while (dialogueReader.IsRunning && dialogueReader.IsWaitingToAdvance && Time.time < startTime + segment.WaitTime) yield return null;

				dialogueReader.ContinuePrompt.Hide();
				textBuilder.Speed = readSpeed;

				if (segment.IsAppended)
					textBuilder.Append(dialogueText, textMode);
				else
					textBuilder.Write(dialogueText, textMode);

				dialogueReader.IsWaitingToAdvance = true;
				while (!CanContinueDialogue(nextSegment, dialogueText, startTime, textBuilder.IsBuilding, isSkippableLine))
				{
					ProcessCompletedText(hasNewHistory, ref hasCapturedHistory);
					yield return null;
				}

				ProcessCompletedText(hasNewHistory, ref hasCapturedHistory);

				dialogueReader.IsWaitingToAdvance = false;
				if (!textBuilder.IsBuilding) break;
			}
		}

		void ProcessCompletedText(bool hasNewHistory, ref bool hasCapturedHistory)
		{
			if (textBuilder.IsBuilding) return;

			if (!dialogueReader.ContinuePrompt.IsVisible)
				dialogueReader.ContinuePrompt.Show();

			if (hasNewHistory && !hasCapturedHistory)
			{
				historyManager.Capture();
				hasCapturedHistory = true;
			}	
		}

		public IEnumerator RunCommands(List<DialogueCommandData.Command> commandList, bool isSkippableLine)
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
				dialogueReader.IsWaitingToAdvance = true;
				while (true)
				{
					// Stop when all processes end, or the user clicks to skip them
					if (processesToWait.All(p => p.IsCompleted)) break;
					else if (!dialogueReader.IsRunning || !dialogueReader.IsWaitingToAdvance || isSkippableLine)
					{
						commandManager.SkipCommands();
						break;
					}
					yield return null;
				}

				// Wait for any previous skipped transitions to complete smoothly
				while (!commandManager.IsIdle()) yield return null;
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
			if (character is not GraphicsCharacter graphicsCharacter || (float.IsNaN(xPos) && float.IsNaN(yPos))) return;

			graphicsCharacter.SetPosition(xPos, yPos, false);
		}

		void ChangeSpeakerGraphics(Character character, Dictionary<SpriteLayerType, string> graphics)
		{
			if (graphics == null) return;

			if (character is SpriteCharacter spriteCharacter)
			{
				foreach (var layer in graphics)
					spriteCharacter.SetSprite(layer.Value, layer.Key);
			}
			else if (character is Model3DCharacter model3DCharacter)
			{
				foreach (string expressionName in graphics.Values)
					model3DCharacter.SetExpression(expressionName);
			}
		}

		bool CanContinueDialogue(DialogueTextData.Segment nextSegment, string text, float startTime, bool isBuildingText, bool isSkippableLine)
		{
			if (!dialogueReader.IsRunning || !dialogueReader.IsWaitingToAdvance) return true;

			bool canContinue = false;

			if (isSkippableLine)
				canContinue = !isBuildingText && Time.time >= startTime + GetSkipReadDelay();
			else if (nextSegment != null && nextSegment.IsAuto)
				canContinue = !isBuildingText;
			else if (dialogueManager.ReadMode == DialogueReadMode.Auto)
				canContinue = !isBuildingText && Time.time >= startTime + GetAutoReadDelay(text.Length);

			if (!canContinue && !isBuildingText && !dialogueReader.ContinuePrompt.IsVisible)
				dialogueReader.ContinuePrompt.Show();

			return canContinue;
		}

		bool IsSkippableLine(string filePath, int lineNumber)
		{
			if (dialogueManager.ReadMode != DialogueReadMode.Skip) return false;

			switch (gameState.SkipMode)
			{
				case DialogueSkipMode.Read:
					return dialogueManager.State.HasReadLine(DialogueUtilities.GetDialogueLineId(filePath, lineNumber));
				case DialogueSkipMode.Unread:
					return dialogueReader.LogicReader.LogicSegmentType == BlockingLogicSegmentType.None;
				case DialogueSkipMode.AfterChoices:
				default:
					return true;
			}
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
