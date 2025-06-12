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
		[SerializeField] DialogueContinuePrompt continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;
		[SerializeField] string dialogueFileName; // TODO get dynamically

		DialogueReader dialogueReader;
		DialogueFile dialogueFile;
		DialogueReadMode readMode = DialogueReadMode.Wait;

		public DialogueReadMode ReadMode => readMode;

		public GameOptionsSO GetGameOptions() => gameOptions;
		public VisualNovelUI GetVisualNovelUI() => visualNovelUI;
		public CharacterManager GetCharacterManager() => characterManager;
		public CommandManager GetCommandManager() => commandManager;
		public DialogueContinuePrompt GetContinuePrompt() => continuePrompt;
		public DialogueTagDirectorySO GetTagDirectory() => tagDirectory;

		void HandleOnAdvanceInput() => HandleInputEvent(DialogueReadMode.Wait, InputActionDuration.Toggle);
		void HandleOnAutoInput() => HandleInputEvent(DialogueReadMode.Auto, InputActionDuration.Toggle);
		void HandleOnSkipInput() => HandleInputEvent(DialogueReadMode.Skip, InputActionDuration.Toggle);
		void HandleOnSkipHoldInput() => HandleInputEvent(DialogueReadMode.Skip, InputActionDuration.Hold);
		void HandleOnSkipHoldEndInput() => HandleInputEvent(DialogueReadMode.Skip, InputActionDuration.End);

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

		void HandleInputEvent(DialogueReadMode newReadMode, InputActionDuration inputDuration)
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
			inputManager.OnAdvance += HandleOnAdvanceInput;
			inputManager.OnAuto += HandleOnAutoInput;
			inputManager.OnSkip += HandleOnSkipInput;
			inputManager.OnSkipHold += HandleOnSkipHoldInput;
			inputManager.OnSkipHoldEnd += HandleOnSkipHoldEndInput;
		}

		void UnsubscribeEvents()
		{
			inputManager.OnAdvance -= HandleOnAdvanceInput;
			inputManager.OnAuto -= HandleOnAutoInput;
			inputManager.OnSkip -= HandleOnSkipInput;
			inputManager.OnSkipHold -= HandleOnSkipHoldInput;
			inputManager.OnSkipHoldEnd -= HandleOnSkipHoldEndInput;
		}
	}
}
