using Characters;
using Commands;
using GameIO;
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
		[SerializeField] DialogueTagDirectorySO tagDirectory;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] VisualNovelUI visualNovelUI;
		[SerializeField] DialogueContinuePromptUI continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;
		[SerializeField] string dialogueFileName; // TODO get dynamically

		DialogueReader dialogueReader;
		DialogueFile dialogueFile;
		DialogueReadMode readMode = DialogueReadMode.Wait;
		string lastInput = "";

		public DialogueReadMode ReadMode => readMode;

		public GameOptionsSO GetGameOptions() => gameOptions;
		public VisualNovelUI GetVisualNovelUI() => visualNovelUI;
		public CharacterManager GetCharacterManager() => characterManager;
		public CommandManager GetCommandManager() => commandManager;
		public DialogueContinuePromptUI GetContinuePrompt() => continuePrompt;
		public DialogueTagDirectorySO GetTagDirectory() => tagDirectory;

		void Start()
		{
			SubscribeEvents();

			dialogueReader = new DialogueReader(this);
		}

		void OnDestroy()
		{
			UnsubscribeEvents();
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.Return))
			{
				//StartCoroutine(RunTest());

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

		void HandleOnAdvanceEvent() => HandleReadModeEvent(DialogueReadMode.Wait, InputActionDuration.Toggle);
		void HandleOnAutoEvent() => HandleReadModeEvent(DialogueReadMode.Auto, InputActionDuration.Toggle);
		void HandleOnSkipEvent() => HandleReadModeEvent(DialogueReadMode.Skip, InputActionDuration.Toggle);
		void HandleOnSkipHoldEvent() => HandleReadModeEvent(DialogueReadMode.Skip, InputActionDuration.Hold);
		void HandleOnSkipHoldEndEvent() => HandleReadModeEvent(DialogueReadMode.Skip, InputActionDuration.End);
		void HandleReadModeEvent(DialogueReadMode newReadMode, InputActionDuration inputDuration)
		{
			DialogueReadMode currentReadMode = readMode;

			if (newReadMode == DialogueReadMode.Wait)
				HandleWaitModeInput();
			else if (inputDuration == InputActionDuration.Toggle)
				HandleToggleInput(newReadMode);
			else if (inputDuration == InputActionDuration.Hold || inputDuration == InputActionDuration.End)
				HandleHoldInput(newReadMode, inputDuration);

			if (currentReadMode != readMode)
			{
				dialogueReader.UpdateReadMode(readMode);

				if (readMode == DialogueReadMode.Auto || readMode == DialogueReadMode.Skip)
					readModeIndicator.Show(readMode);
				else
					readModeIndicator.Hide();
			}
		}

		void HandleOnClearInputEvent() => HandleInputEvent("");
		void HandleOnSubmitInputEvent(string input) => HandleInputEvent(input);
		void HandleInputEvent(string input)
		{
			lastInput = input;
		}

		void HandleWaitModeInput()
		{
			if (readMode == DialogueReadMode.Wait)
			{
				dialogueReader.AdvanceDialogue();
			}
			else if (readMode == DialogueReadMode.Auto)
			{
				if (gameOptions.Dialogue.StopAutoOnClick)
					readMode = DialogueReadMode.Wait;
				else
					dialogueReader.AdvanceDialogue();
			}
			else if (readMode == DialogueReadMode.Skip)
			{
				readMode = DialogueReadMode.Wait;
			}
		}

		void HandleToggleInput(DialogueReadMode newReadMode)
		{
			readMode = readMode == newReadMode ? DialogueReadMode.Wait : newReadMode;
		}

		void HandleHoldInput(DialogueReadMode newReadMode, InputActionDuration inputDuration)
		{
			readMode = inputDuration == InputActionDuration.Hold ? newReadMode : DialogueReadMode.Wait;
		}

		void SubscribeEvents()
		{
			inputManager.OnAdvance += HandleOnAdvanceEvent;
			inputManager.OnAuto += HandleOnAutoEvent;
			inputManager.OnSkip += HandleOnSkipEvent;
			inputManager.OnSkipHold += HandleOnSkipHoldEvent;
			inputManager.OnSkipHoldEnd += HandleOnSkipHoldEndEvent;
			inputManager.OnClearInput += HandleOnClearInputEvent;
			inputManager.OnSubmitInput += HandleOnSubmitInputEvent;
		}

		void UnsubscribeEvents()
		{
			inputManager.OnAdvance -= HandleOnAdvanceEvent;
			inputManager.OnAuto -= HandleOnAutoEvent;
			inputManager.OnSkip -= HandleOnSkipEvent;
			inputManager.OnSkipHold -= HandleOnSkipHoldEvent;
			inputManager.OnSkipHoldEnd -= HandleOnSkipHoldEndEvent;
			inputManager.OnClearInput -= HandleOnClearInputEvent;
			inputManager.OnSubmitInput -= HandleOnSubmitInputEvent;
		}

		// TODO Remove test function
		IEnumerator RunTest()
		{
			yield return characterManager.CreateCharacter("v");

			SpriteCharacter v = characterManager.GetCharacter("v") as SpriteCharacter;

			yield return visualNovelUI.Show();

			yield return v.Show();
			yield return v.Say("Let me test the input for you~");
			yield return v.Say("Tell me...");

			yield return visualNovelUI.ShowInput("What's your name...?");

			while (lastInput == string.Empty) yield return null;

			v.Say($"I knew it was you, {lastInput}.");
		}
	}
}
