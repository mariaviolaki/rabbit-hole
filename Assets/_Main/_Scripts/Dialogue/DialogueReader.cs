using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Dialogue
{
	public class DialogueReader
	{
		const string CommentLineDelimiter = "//";
		readonly DialogueSystem dialogueSystem;
		readonly TextBuilder textBuilder;
		readonly CharacterManager characterManager;

		Coroutine readProcess;

		public bool IsRunning { get; set; }

		bool IsValidDialogueLine(string line) => !string.IsNullOrEmpty(line) && !line.StartsWith(CommentLineDelimiter);

		public DialogueReader(DialogueSystem dialogueSystem, CharacterManager characterManager, DialogueUI dialogueUI)
		{
			this.dialogueSystem = dialogueSystem;
			this.characterManager = characterManager;
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

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
				{
					dialogueSystem.SetSpeaker(dialogueLine.Speaker);
					yield return DisplayDialogue(dialogueLine);
				}

				if (dialogueLine.Commands != null)
					yield return dialogueSystem.RunCommands(dialogueLine.Commands.CommandList);
			}

			IsRunning = false;
			yield return new WaitUntil(() => IsRunning);
		}

		// Read a list of dialogue lines spoken by a certain character
		IEnumerator Read(string speakerName, List<string> lines)
		{
			Character character = characterManager.GetCharacter(speakerName);
			dialogueSystem.SetSpeakerName(character.Data);

			foreach (string line in lines)
			{
				if (!IsValidDialogueLine(line)) continue;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
					yield return DisplayDialogue(dialogueLine);
			}

			IsRunning = false;
			yield return new WaitUntil(() => IsRunning);
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
	}
}
