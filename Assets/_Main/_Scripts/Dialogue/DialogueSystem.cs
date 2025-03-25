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
			Task zTask = characterManager.CreateCharacter("Zero");
			Task egTask = characterManager.CreateCharacter("Eccentric Guy");
			Task v2Task = characterManager.CreateCharacter("Mirror Void", "Void");

			yield return new WaitUntil(() => Task.WhenAll(vTask, zTask, egTask, v2Task).IsCompleted);

			SpriteCharacter v = characterManager.GetCharacter("Void") as SpriteCharacter;
			SpriteCharacter z = characterManager.GetCharacter("Zero") as SpriteCharacter;
			SpriteCharacter eg = characterManager.GetCharacter("Eccentric Guy") as SpriteCharacter;
			SpriteCharacter v2 = characterManager.GetCharacter("Mirror Void") as SpriteCharacter;

			v.SetSprite(SpriteLayerType.Body, "Void Body Casual");
			v.SetSprite(SpriteLayerType.Face, "Void Face Neutral");

			v.SetPosition(new Vector2(0, 0));
			v.Show();
			yield return v.Say("Testing,{a 0.5} testing...");

			yield return v.MoveToPosition(new Vector2(0.15f, 0), 10);
			yield return new WaitForSeconds(1);
			yield return v.MoveToPosition(new Vector2(0.15f, 0), 10);
			yield return new WaitForSeconds(1);
			yield return v.MoveToPosition(new Vector2(0.5f, 0), 10);
			yield return new WaitForSeconds(1);
			yield return v.MoveToPosition(new Vector2(1, 0), 10);
			yield return new WaitForSeconds(1);
			yield return v.MoveToPosition(new Vector2(0, 0), 10);
		}
	}
}
