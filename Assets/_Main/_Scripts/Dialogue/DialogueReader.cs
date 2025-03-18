using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	public class DialogueReader
	{
		readonly DialogueSystem dialogueSystem;
		readonly TextBuilder textBuilder;

		Coroutine readProcess;

		public bool IsRunning { get; set; }

		public DialogueReader(DialogueSystem dialogueSystem, DialogueUI dialogueUI)
		{
			this.dialogueSystem = dialogueSystem;
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

			textBuilder.Speed = dialogueSystem.GameOptions.TextSpeed;

			// Start paused and wait for player input
			IsRunning = true;
		}

		// Read lines directly from dialogue files (each line includes: speaker, dialogue, commands)
		IEnumerator Read(List<string> lines)
		{
			foreach (string line in lines)
			{
				if (string.IsNullOrEmpty(line)) continue;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
					yield return DisplayDialogue(dialogueLine, false);

				if (dialogueLine.Commands != null)
					yield return dialogueSystem.RunCommands(dialogueLine.Commands.CommandList);
			}

			IsRunning = false;
			yield return new WaitUntil(() => IsRunning);
		}

		// Read a list of dialogue lines spoken by a certain character
		IEnumerator Read(string speakerName, List<string> lines)
		{
			dialogueSystem.SetSpeaker(speakerName);

			foreach (string line in lines)
			{
				if (string.IsNullOrEmpty(line)) continue;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
					yield return DisplayDialogue(dialogueLine, true);
			}

			IsRunning = false;
			yield return new WaitUntil(() => IsRunning);
		}

		IEnumerator DisplayDialogue(DialogueLine line, bool isSpeakerSet)
		{
			foreach (DialogueTextData.Segment segment in line.Dialogue.Segments)
			{
				yield return WaitForNextDialogueSegment(segment);

				if (!isSpeakerSet)
				{
					dialogueSystem.SetSpeaker(line.Speaker?.Name);
					isSpeakerSet = true;
				}

				yield return DisplayDialogueSegment(segment);
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
					textBuilder.Append(segment.Text, dialogueSystem.GameOptions.TextMode);
				else
					textBuilder.Write(segment.Text, dialogueSystem.GameOptions.TextMode);

				IsRunning = false;
				yield return new WaitUntil(() => IsRunning || !textBuilder.IsBuilding);

				if (!textBuilder.IsBuilding) break;
			}
		}
	}
}
