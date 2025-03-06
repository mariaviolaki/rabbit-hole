using Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	public class DialogueWriter
	{
		DialogueSystem dialogueSystem;
		DialogueUI dialogueUI;
		CommandManager commandManager;
		TextBuilder textBuilder;

		Coroutine readProcess;

		public bool IsRunning { get; set; }

		public DialogueWriter(DialogueSystem dialogueSystem, DialogueUI dialogueUI, CommandManager commandManager)
		{
			this.dialogueSystem = dialogueSystem;
			this.dialogueUI = dialogueUI;
			this.commandManager = commandManager;

			textBuilder = new TextBuilder(dialogueUI.DialogueText);
		}

		public void StartWriting(List<string> lines)
		{
			if (lines == null) return;

			if (readProcess != null)
			{
				dialogueSystem.StopCoroutine(readProcess);
			}

			// Start paused and wait for player input
			IsRunning = false;

			readProcess = dialogueSystem.StartCoroutine(Read(lines));
		}

		public IEnumerator Read(List<string> lines)
		{
			yield return new WaitUntil(() => IsRunning);

			textBuilder.Speed = dialogueSystem.TextSpeed;

			foreach (string line in lines)
			{
				Debug.Log($"Parsing: {line}");

				if (string.IsNullOrEmpty(line)) continue;

				DialogueLine dialogueLine = DialogueParser.Parse(line);

				if (dialogueLine.Dialogue != null)
					yield return DisplayDialogue(dialogueLine);

				if (dialogueLine.Commands != null)
					yield return RunCommands(dialogueLine);
			}
		}

		IEnumerator RunCommands(DialogueLine line)
		{
			foreach (DialogueCommandData.Command command in line.Commands.CommandList)
			{
				if (command.IsWaiting)
					yield return commandManager.Execute(command.Name, command.Arguments);
				else
					commandManager.Execute(command.Name, command.Arguments);
			}
		}

		IEnumerator DisplayDialogue(DialogueLine line)
		{
			bool isSpeakerSet = false;

			foreach (DialogueTextData.Segment segment in line.Dialogue.Segments)
			{
				yield return WaitForNextDialogueSegment(segment);

				if (!isSpeakerSet)
				{
					SetSpeaker(line.Speaker.DisplayName);
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
					textBuilder.Append(segment.Text, dialogueSystem.TextMode);
				else
					textBuilder.Write(segment.Text, dialogueSystem.TextMode);

				IsRunning = false;
				yield return new WaitUntil(() => IsRunning || !textBuilder.IsBuilding);

				if (!textBuilder.IsBuilding) break;
			}
		}

		void SetSpeaker(string speakerName)
		{
			if (speakerName == null)
				dialogueUI.HideSpeaker();
			else
				dialogueUI.ShowSpeaker(speakerName);
		}
	}
}