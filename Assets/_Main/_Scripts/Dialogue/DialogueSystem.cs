using Characters;
using Commands;
using System.Collections;
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

			yield return new WaitUntil(() => Task.WhenAll(vTask, mTask).IsCompleted);

			SpriteCharacter v = characterManager.GetCharacter("Void") as SpriteCharacter;
			v.SetPositionInstant(new Vector2(1f, 0.5f));
			v.Show();

			Model3DCharacter m = characterManager.GetCharacter("Marsh") as Model3DCharacter;
			m.SetPositionInstant(new Vector2(0f, 0.5f));
			m.Show();

			yield return m.Say("Testing...");
			v.SetPosition(new Vector2(0f, 0.5f));
			m.SetPosition(new Vector2(1f, 0.5f));
			yield return m.Say("Testing...");
			v.SetColor(Color.blue);
			m.SetColor(Color.blue);
			yield return m.Say("Testing...");
			v.Flip();
			m.Flip();
			yield return m.Say("Testing...");
			v.FaceLeft();
			m.FaceLeft();
			yield return m.Say("Testing...");
			v.FaceRight();
			m.FaceRight();
			yield return m.Say("Testing...");
			v.Highlight();
			m.Highlight();
			yield return m.Say("Testing...");
			v.Unhighlight();
			m.Unhighlight();
			yield return m.Say("Testing...");
			v.Hide();
			m.Hide();
			yield return m.Say("Testing...");
			v.Show();
			m.Show();
			yield return m.Say("Testing...");
			v.SetColor(Color.white);
			m.SetColor(Color.white);
			yield return m.Say("Testing...");
			v.SetSprite(SpriteLayerType.Body, "Void Body Yukata");
			v.SetSprite(SpriteLayerType.Face, "Void Face Evil");
			yield return m.SetExpression("Evil");
			m.SetMotion("Kiss");
			yield return m.Say("Testing...");
			v.Highlight();
			m.Highlight();
		}
	}
}
