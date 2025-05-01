using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Commands;
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
		readonly DialogueUI dialogueUI;

		Coroutine readProcess;

		public bool IsRunning { get; set; }

		bool IsValidDialogueLine(string line) => !string.IsNullOrEmpty(line) && !line.StartsWith(CommentLineDelimiter);

		public DialogueReader(DialogueSystem dialogueSystem, CharacterManager characterManager, CommandManager commandManager, DialogueUI dialogueUI)
		{
			this.dialogueSystem = dialogueSystem;
			this.characterManager = characterManager;
			this.commandManager = commandManager;
			this.dialogueUI = dialogueUI;
			textBuilder = new TextBuilder(dialogueUI.DialogueText);
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
			foreach (DialogueTextData.Segment segment in line.Dialogue.Segments)
			{
				yield return DisplayDialogueSegment(segment);
				yield return WaitForNextDialogueSegment(segment);
			}
		}

		IEnumerator WaitForNextDialogueSegment(DialogueTextData.Segment segment)
		{
			if (segment.IsAuto)
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
				dialogueUI.HideSpeaker();
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
				if (command.IsWaiting || command.Name == "Wait")
					processesToWait.Add(commandManager.Execute(command.Name, command.Arguments));
				else
					commandManager.Execute(command.Name, command.Arguments);
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
			dialogueUI.ShowSpeaker(characterData);
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
