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
			yield return v.SetSprite(SpriteLayerType.Body, "Void Body Yukata");
			yield return v.SetSprite(SpriteLayerType.Face, "Void Face Evil");
			v.SetPosition(new Vector2(0.5f, 0));
			yield return v.FaceLeft(100);
			v.Show();

			yield return v.Say("Testing...");
			v.Animate("HopOnce");
			yield return v.Say("Testing...");
			yield return v.SetSprite(SpriteLayerType.Face, "Void Face Flustered");
			v.Flip();
			v.Animate("HopOnce");
			yield return v.Say("Testing...");
			v.SetSprite(SpriteLayerType.Face, "Void Face Evil");
			v.Animate("Hop", true);
			yield return v.Say("Testing...");
			v.Animate("Hop", false);
			yield return v.Say("Testing...");
			v.Animate("Shiver", true);
			yield return v.Say("Testing...");
			v.Animate("Shiver", false);
			yield return v.Say("Testing...");
			v.Darken();
			yield return v.Say("Testing...");
			v.Lighten();
			v.Animate("Enlarge");
			yield return v.Say("Testing...");
			v.Darken();
			v.Animate("Shrink");
			yield return v.Say("Testing...");
		}
	}
}
