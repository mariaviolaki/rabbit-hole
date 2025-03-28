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
			Task z2Task = characterManager.CreateCharacter("Mirror Zero", "Zero");
			Task eg2Task = characterManager.CreateCharacter("Mirror Eccentric Guy", "Eccentric Guy");
			Task r1Task = characterManager.CreateCharacter("Random Guy 1");

			yield return new WaitUntil(() => Task.WhenAll(vTask, zTask, egTask, v2Task, r1Task, z2Task, eg2Task).IsCompleted);

			SpriteCharacter v = characterManager.GetCharacter("Void") as SpriteCharacter;
			SpriteCharacter z = characterManager.GetCharacter("Zero") as SpriteCharacter;
			SpriteCharacter eg = characterManager.GetCharacter("Eccentric Guy") as SpriteCharacter;
			SpriteCharacter v2 = characterManager.GetCharacter("Mirror Void") as SpriteCharacter;
			SpriteCharacter z2 = characterManager.GetCharacter("Mirror Zero") as SpriteCharacter;
			SpriteCharacter eg2 = characterManager.GetCharacter("Mirror Eccentric Guy") as SpriteCharacter;
			TextCharacter r1 = characterManager.GetCharacter("Random Guy 1") as TextCharacter;
			TextCharacter r2 = characterManager.GetCharacter("Random Guy 2") as TextCharacter;

			v.SetSprite(SpriteLayerType.Body, "Void Body Yukata");
			v.SetSprite(SpriteLayerType.Face, "Void Face Evil");
			z.SetSprite(SpriteLayerType.Body, "Zero Body Comfy");

			v.SetPosition(new Vector2(0.5f, 0));
			v.Show();
			v2.SetPosition(new Vector2(0.6f, 0));
			v2.Show();
			z.SetPosition(new Vector2(0.4f, 0));
			z.Show();
			eg.SetPosition(new Vector2(0.55f, 0));
			eg.Show();

			yield return v.Say("Testing...");
			z2.SetPriority(0);
			yield return v.Say("Testing...");
			v.SetPriority(0);
			yield return v.Say("Testing...");
			v2.SetPriority(0);
			yield return v.Say("Testing...");
			z.SetPriority(1);
			yield return v.Say("Testing...");
			v.SetPriority(100);
			yield return v.Say("Testing...");
			v.SetPriority(2);
			yield return v.Say("Testing...{a 0.5} Testing...");

			characterManager.SetPriority(new string[] { null, "Void", "Zero", "Random Guy 1", "Eccentric Guy" });
		}
	}
}
