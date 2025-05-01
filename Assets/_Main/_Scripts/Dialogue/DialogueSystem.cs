using Characters;
using Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		[SerializeField] DialogueUI dialogueUI;
		[SerializeField] string dialogueFileName; // TODO get dynamically

		DialogueReader dialogueReader;
		DialogueFile dialogueFile;

		public GameOptionsSO GameOptions { get { return gameOptions; } }

		void Start()
		{
			dialogueReader = new DialogueReader(this, characterManager, commandManager, dialogueUI);

			inputManager.OnAdvance += AdvanceDialogue;
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StartDialogueFromFile();
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

		async Task<Coroutine> StartDialogueFromFile()
		{
			TextAsset dialogueAsset = await fileManager.LoadDialogueFile(dialogueFileName);
			if (dialogueAsset == null) return null;

			dialogueFile = new DialogueFile(dialogueAsset.name, dialogueAsset.text);
			return dialogueReader.StartReading(dialogueFile.Lines);
		}

		void AdvanceDialogue()
		{
			if (dialogueReader.IsRunning) return;

			dialogueReader.IsRunning = true;
		}
	}
}
