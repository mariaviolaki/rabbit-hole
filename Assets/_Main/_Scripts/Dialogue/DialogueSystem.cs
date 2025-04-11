using Characters;
using Commands;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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

		// TODO Delete after testing
		[SerializeField] TMP_FontAsset testFont;

		DialogueReader dialogueReader;
		DialogueFile dialogueFile;

		public GameOptionsSO GameOptions { get { return gameOptions; } }

		void Start()
		{
			dialogueReader = new DialogueReader(this, dialogueUI);

			inputManager.OnAdvance += AdvanceDialogue;
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StartDialogueFromFile();
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

		// TODO remove test function
		IEnumerator RunTest()
		{
			Task vTask = characterManager.CreateCharacter("Void");
			Task mTask = characterManager.CreateCharacter("Marsh");
			Task m2Task = characterManager.CreateCharacter("Marsh 2", "Marsh");
			Task m3Task = characterManager.CreateCharacter("Marsh 3", "Marsh");

			yield return new WaitUntil(() => Task.WhenAll(vTask, mTask, m2Task, m3Task).IsCompleted);

			SpriteCharacter v = characterManager.GetCharacter("Void") as SpriteCharacter;
			v.SetPosition(new Vector2(1f, 0.5f));
			v.Show();

			Model3DCharacter m = characterManager.GetCharacter("Marsh") as Model3DCharacter;
			m.SetPosition(new Vector2(0f, 0.5f));
			m.Show();

			Model3DCharacter m2 = characterManager.GetCharacter("Marsh 2") as Model3DCharacter;
			m2.SetPosition(new Vector2(0.2f, 0.5f));
			m2.Show();

			Model3DCharacter m3 = characterManager.GetCharacter("Marsh 3") as Model3DCharacter;
			m3.SetPosition(new Vector2(0.4f, 0.5f));
			m3.Show();

			yield return new WaitForSeconds(1f);

			m.SetMotion("Kiss");
			m2.SetMotion("Pointing");
			m3.SetMotion("Angry");

			yield return m.Say("Testing...");
		}
	}
}
