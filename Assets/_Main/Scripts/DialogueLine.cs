using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLine
{
	string speaker;
	string dialogue;
	string commands;

    public DialogueLine(string speaker, string dialogue, string commands)
	{
		this.speaker = speaker;
		this.dialogue = dialogue;
		this.commands = commands;
	}

	// TODO Remove debug function
	public void Print()
	{
		Debug.Log($"Speaker: {speaker}\nDialogue: {dialogue}\nCommands: {commands}");
	}
}
