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
		readonly DialogueSystem dialogueSystem;
		readonly TextBuilder textBuilder;
		readonly CharacterManager characterManager;
		readonly CommandManager commandManager;
		readonly VisualNovelUI visualNovelUI;
		readonly DialogueContinuePrompt continuePrompt;

		Coroutine readProcess;

		public bool IsRunning { get; set; }

		bool IsValidDialogueLine(string line) => !string.IsNullOrEmpty(line) && !line.StartsWith(CommentLineDelimiter);

		public DialogueReader(DialogueSystem dialogueSystem)
		{
			this.dialogueSystem = dialogueSystem;
			characterManager = dialogueSystem.GetCharacterManager();
			commandManager = dialogueSystem.GetCommandManager();
			visualNovelUI = dialogueSystem.GetVisualNovelUI();
			continuePrompt = dialogueSystem.GetContinuePrompt();
			textBuilder = new TextBuilder(visualNovelUI.DialogueText);
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

			textBuilder.Speed = dialogueSystem.GameOptions.Dialogue.TextSpeed;

			// Start paused and wait for player input
			IsRunning = true;
		}

		// Read lines directly from dialogue files (each line includes: speaker, dialogue, commands)
		IEnumerator Read(List<string> lines)
		{
			foreach (string line in lines)
			{
				if (!IsValidDialogueLine(line)) continue;

				// Wait for any previous skipped transitions to complete smoothly
				yield return new WaitUntil(() => commandManager.IsIdle);

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

				yield return DisplayDialogueSegment(segment);
				continuePrompt.Show();
				yield return WaitForNextDialogueSegment(nextSegment);
				continuePrompt.Hide();
			}
		}

		IEnumerator WaitForNextDialogueSegment(DialogueTextData.Segment segment)
		{
			if (segment != null && segment.IsAuto)
			{
				float startTime = Time.time;
				yield return new WaitUntil(() => IsRunning || Time.time >= startTime + segment.WaitTime);
			}
			else
			{
				yield return new WaitUntil(() => IsRunning);
			}
		}

		IEnumerator DisplayDialogueSegment(DialogueTextData.Segment segment)
		{
			while (true)
			{
				if (segment.IsAppended)
					textBuilder.Append(segment.Text, dialogueSystem.GameOptions.Dialogue.TextMode);
				else
					textBuilder.Write(segment.Text, dialogueSystem.GameOptions.Dialogue.TextMode);

				IsRunning = false;
				yield return new WaitUntil(() => IsRunning || !textBuilder.IsBuilding);

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
					yield return new WaitUntil(() => process.IsCompleted);
				else if (command.IsWaiting || command.Name == "Wait")
					processesToWait.Add(process);						
			}

			// Wait to execute all processes of this line concurrently
			if (processesToWait.Count > 0)
			{
				IsRunning = false;
				while (true)
				{
					// Stop when all processes end, or the user clicks to skip them
					if (processesToWait.All(p => p.IsCompleted)) break;
					else if (IsRunning)
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
	}
}
