using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Commands;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueReader
	{
		const string CommentLineDelimiter = "//";
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
		readonly DialogueTagManager tagManager;

		Coroutine readProcess;

		TextBuilder.TextMode textMode;
		bool isRunning = false;

		public void AdvanceDialogue() => isRunning = true;
		public void PauseDialogue() => isRunning = false;

		bool IsValidDialogueLine(string line) => !string.IsNullOrEmpty(line) && !line.StartsWith(CommentLineDelimiter);

		public DialogueReader(DialogueSystem dialogueSystem)
		{
			this.dialogueSystem = dialogueSystem;
			gameOptions = dialogueSystem.GetGameOptions();
			characterManager = dialogueSystem.GetCharacterManager();
			commandManager = dialogueSystem.GetCommandManager();
			visualNovelUI = dialogueSystem.GetVisualNovelUI();
			continuePrompt = dialogueSystem.GetContinuePrompt();

			textBuilder = new TextBuilder(visualNovelUI.DialogueText);
			tagManager = new DialogueTagManager(dialogueSystem.GetTagDirectory());
			textMode = gameOptions.Dialogue.TextMode;
		}

		public void UpdateReadMode(DialogueReadMode readMode)
		{
			if (readMode == DialogueReadMode.Skip)
				textBuilder.Speed = textBuilder.MaxSpeed;
			else
				textBuilder.Speed = gameOptions.Dialogue.TextSpeed;
		}

		public Coroutine StartReading(List<string> lines)
		{
			if (lines == null) return null;

			PrepareDialogue();

			readProcess = dialogueSystem.StartCoroutine(Read(lines));
			return readProcess;
		}

		public Coroutine StartReading(string speakerName, List<string> lines)
		{
			if (lines == null) return null;

			PrepareDialogue();

			readProcess = dialogueSystem.StartCoroutine(Read(speakerName, lines));
			return readProcess;
		}

		void PrepareDialogue()
		{
			// End any prior writing process
			if (readProcess != null)
				dialogueSystem.StopCoroutine(readProcess);

			UpdateReadMode(DialogueReadMode.Wait);
			isRunning = true;
		}

		// Read lines directly from dialogue files (each line includes: speaker, dialogue, commands)
		IEnumerator Read(List<string> lines)
		{
			foreach (string line in lines)
			{
				if (!IsValidDialogueLine(line)) continue;

				// Wait for any previous skipped transitions to complete smoothly
				while (!commandManager.IsIdle) yield return null;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
				{
					SetSpeaker(dialogueLine.Speaker);
					yield return DisplayDialogue(dialogueLine);
				}

				if (dialogueLine.Commands != null)
					yield return RunCommands(dialogueLine.Commands.CommandList);
			}
		}

		// Read a list of dialogue lines spoken by a certain character
		IEnumerator Read(string speakerName, List<string> lines)
		{
			Character character = characterManager.GetCharacter(speakerName);
			SetSpeakerName(character.Data);

			foreach (string line in lines)
			{
				if (!IsValidDialogueLine(line)) continue;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
					yield return DisplayDialogue(dialogueLine);
			}
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
			string dialogueText = tagManager.Parse(segment.Text);

			while (true)
			{
				float startTime = Time.time;

				// Wait for a specified duration before showing the text (unless forced to continue)
				if (segment != null && segment.IsAuto && dialogueSystem.ReadMode != DialogueReadMode.Skip)
					while (!isRunning && Time.time < startTime + segment.WaitTime) yield return null;

				continuePrompt.Hide();

				if (segment.IsAppended)
					textBuilder.Append(dialogueText, textMode);
				else
					textBuilder.Write(dialogueText, textMode);

				PauseDialogue();
				while (!CanContinueDialogue(nextSegment, dialogueText, startTime, textBuilder.IsBuilding)) yield return null;

				continuePrompt.Show();

				if (!textBuilder.IsBuilding) break;
			}
		}

		public void SetSpeaker(DialogueSpeakerData speakerData)
		{
			if (speakerData == null || string.IsNullOrEmpty(speakerData.Name))
			{
				visualNovelUI.HideSpeaker();
				return;
			}

			Character character = characterManager.GetCharacter(speakerData.Name);

			string tagValue = tagManager.GetTagValue(character.Data.Name);
			if (tagValue != null)
				speakerData.DisplayName = tagManager.Parse(character.Data.Name);

			ChangeSpeakerDisplayName(character, speakerData.DisplayName);
			ChangeSpeakerPosition(character, speakerData.XPos, speakerData.YPos);
			ChangeSpeakerGraphics(character, speakerData.Layers);
			SetSpeakerName(character.Data);
		}

		public IEnumerator RunCommands(List<DialogueCommandData.Command> commandList)
		{
			List<CommandProcess> processesToWait = new List<CommandProcess>();

			foreach (DialogueCommandData.Command command in commandList)
			{
				CommandProcess process = commandManager.Execute(command.Name, command.Arguments);
				if (process == null) continue;

				if (process.IsTask)
					while (!process.IsCompleted) yield return null;
				else if (command.IsWaiting || command.Name == "Wait")
					processesToWait.Add(process);						
			}

			// Wait to execute all processes of this line concurrently
			if (processesToWait.Count > 0)
			{
				PauseDialogue();
				while (true)
				{
					// Stop when all processes end, or the user clicks to skip them
					if (processesToWait.All(p => p.IsCompleted)) break;
					else if (isRunning)
					{
						commandManager.SkipCommands();
						break;
					}
					yield return null;
				}
			}
		}

		public void SetSpeakerName(CharacterData characterData)
		{
			visualNovelUI.ShowSpeaker(characterData);
		}

		void ChangeSpeakerDisplayName(Character character, string displayName)
		{
			if (string.IsNullOrEmpty(displayName)) return;

			character.SetDisplayName(displayName);
		}

		void ChangeSpeakerPosition(Character character, float xPos, float yPos)
		{
			if (character is not GraphicsCharacter) return;

			if (!float.IsNaN(xPos) && !float.IsNaN(yPos))
				((GraphicsCharacter)character).SetPosition(new Vector2(xPos, yPos));
			else if (!float.IsNaN(xPos))
				((GraphicsCharacter)character).SetPositionX(xPos);
			else if (!float.IsNaN(yPos))
				((GraphicsCharacter)character).SetPositionY(yPos);
		}

		void ChangeSpeakerGraphics(Character character, Dictionary<SpriteLayerType, string> graphics)
		{
			if (graphics == null) return;

			if (character is SpriteCharacter)
			{
				foreach (var layer in graphics)
					((SpriteCharacter)character).SetSprite(layer.Key, layer.Value);
			}
			else if (character is Model3DCharacter)
			{
				foreach (string expressionName in graphics.Values)
					((Model3DCharacter)character).SetExpression(expressionName);
			}
		}

		bool CanContinueDialogue(DialogueTextData.Segment nextSegment, string text, float startTime, bool isBuildingText)
		{
			if (isRunning) return true;

			bool canContinue = false;

			if (nextSegment != null && nextSegment.IsAuto)
				canContinue = !isBuildingText;
			else if (dialogueSystem.ReadMode == DialogueReadMode.Wait)
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
