using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueWriter
{
	DialogueSystem dialogueSystem;
	DialogueUI dialogueUI;
	TextBuilder textBuilder;

	Coroutine readProcess;

	public bool IsRunning { get; set; }

	public DialogueWriter(DialogueSystem dialogueSystem, DialogueUI dialogueUI)
	{
		this.dialogueSystem = dialogueSystem;
		this.dialogueUI = dialogueUI;

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
			if (string.IsNullOrEmpty(line)) continue;

			Debug.Log($"Parsing {line}");
			DialogueLine dialogueLine = DialogueParser.Parse(line);

			if (dialogueLine.Commands != null)
				RunCommands(dialogueLine);

			if (dialogueLine.Dialogue != null)
				yield return DisplayDialogue(dialogueLine);
		}
	}

	void RunCommands(DialogueLine line)
	{
		Debug.Log($"Commands: {line.Commands}");
	}

	IEnumerator DisplayDialogue(DialogueLine line)
	{
		SetSpeaker(line.Speaker);

		do
		{
			textBuilder.Write(line.Dialogue, dialogueSystem.TextMode);

			IsRunning = false;
			yield return new WaitUntil(() => IsRunning);
		}
		while (textBuilder.IsBuilding);
	}

	void SetSpeaker(string speakerName)
	{
		if (speakerName == null)
			dialogueUI.HideSpeaker();
		else
			dialogueUI.ShowSpeaker(speakerName);
	}
}
