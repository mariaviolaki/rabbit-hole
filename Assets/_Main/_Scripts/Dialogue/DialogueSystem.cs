using Characters;
using Commands;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueSystem : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] VisualNovelUI visualNovelUI;
		[SerializeField] DialogueContinuePrompt continuePrompt;
		[SerializeField] string dialogueFileName; // TODO get dynamically

		DialogueReader dialogueReader;
		DialogueFile dialogueFile;

		public GameOptionsSO GameOptions { get { return gameOptions; } }

		public VisualNovelUI GetVisualNovelUI() => visualNovelUI;
		public CharacterManager GetCharacterManager() => characterManager;
		public CommandManager GetCommandManager() => commandManager;
		public DialogueContinuePrompt GetContinuePrompt() => continuePrompt;

		void Start()
		{
			dialogueReader = new DialogueReader(this);

			inputManager.OnAdvance += AdvanceDialogue;
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StartCoroutine(StartDialogueFromFile());
			}
		}

		public Coroutine Say(string speakerName, string line)
		{
			return Say(speakerName, new List<string> { line });
		}

		public Coroutine Say(string speakerName, List<string> lines)
		{
			return dialogueReader.StartReading(speakerName, lines);
		}

		IEnumerator StartDialogueFromFile()
		{
			yield return fileManager.LoadDialogueFile(dialogueFileName);
			TextAsset dialogueAsset = fileManager.GetDialogueFile(dialogueFileName);
			if (dialogueAsset == null) yield break;

			dialogueFile = new DialogueFile(dialogueAsset.name, dialogueAsset.text);
			yield return dialogueReader.StartReading(dialogueFile.Lines);
		}

		void AdvanceDialogue()
		{
			if (dialogueReader.IsRunning) return;

			dialogueReader.IsRunning = true;
		}
	}
}
