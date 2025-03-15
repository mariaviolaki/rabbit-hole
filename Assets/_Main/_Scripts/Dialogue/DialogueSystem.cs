using Characters;
using Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dialogue
{
	public class DialogueSystem : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] DialogueUI dialogueUI;
		[SerializeField] AssetLabelReference dialogueLabel;

		DialogueReader dialogueReader;

		Dictionary<string, DialogueFile> dialogueFiles = new Dictionary<string, DialogueFile>();
		string currentDialogueFile;

		public GameOptionsSO GameOptions { get { return gameOptions; } }

		void Start()
		{
			dialogueReader = new DialogueReader(this, dialogueUI);

			fileManager.OnLoadTextFiles += CacheDialogueFiles;
			inputManager.OnAdvance += AdvanceDialogue;

			LoadDialogueFiles();
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StartDialogue();
			}
			// TODO remove test functionality
			else if (Input.GetKeyDown(KeyCode.T))
			{
				StartCoroutine(RunTest());
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

		public void SetSpeaker(string speakerName)
		{
			if (string.IsNullOrEmpty(speakerName))
			{
				dialogueUI.HideSpeaker();
			}
			else
			{
				Character character = characterManager.GetCharacter(speakerName);
				dialogueUI.ShowSpeaker(character.Data);
			}
		}

		public IEnumerator RunCommands(List<DialogueCommandData.Command> commandList)
		{
			foreach (DialogueCommandData.Command command in commandList)
			{
				if (command.IsWaiting)
					yield return commandManager.Execute(command.Name, command.Arguments);
				else
					commandManager.Execute(command.Name, command.Arguments);
			}
		}

		void LoadDialogueFiles()
		{
			if (dialogueLabel == null) return;

			fileManager.LoadTextFiles(dialogueLabel);
		}

		Coroutine StartDialogue()
		{
			if (currentDialogueFile == null) return null;

			DialogueFile dialogueFile = dialogueFiles[currentDialogueFile];
			return dialogueReader.StartReading(dialogueFile.Lines);
		}

		void AdvanceDialogue()
		{
			if (dialogueReader.IsRunning) return;

			dialogueReader.IsRunning = true;
		}

		void CacheDialogueFiles(List<TextAsset> textAssets)
		{
			currentDialogueFile = textAssets[0].name;

			foreach (TextAsset textAsset in textAssets)
			{
				dialogueFiles[textAsset.name] = new DialogueFile(textAsset.name, textAsset.text);
			}
		}

		// TODO remove test function
		IEnumerator RunTest()
		{
			yield return characterManager.GetCharacter("Void")?.Say("Testing,{a 0.5} testing...");
			yield return characterManager.GetCharacter("Un-Void")?.Say(new List<string> { "Is...", "...this...", "...working?" });
			yield return characterManager.GetCharacter("")?.Say("Error Check 1");
			yield return characterManager.GetCharacter(null)?.Say("Error Check 2");
		}
	}
}
