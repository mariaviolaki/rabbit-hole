using Audio;
using Characters;
using Commands;
using GameIO;
using Visuals;
using History;
using Logic;
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
		[SerializeField] DialogueTagBankSO tagBank;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] AudioManager audioManager;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] VisualGroupManager visualManager;
		[SerializeField] VisualNovelUI visualNovelUI;
		[SerializeField] DialogueContinuePromptUI continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;
		[SerializeField] string dialogueFileName; // TODO get dynamically

		DialogueTagManager tagManager;
		ScriptVariableManager variableManager;
		DialogueReader dialogueReader;
		DialogueReadMode readMode = DialogueReadMode.Wait;
		Coroutine dialogueLoadProcess;

		public GameOptionsSO Options => gameOptions;
		public VisualNovelUI UI => visualNovelUI;
		public CharacterManager Characters => characterManager;
		public CommandManager Commands => commandManager;
		public DialogueContinuePromptUI ContinuePrompt => continuePrompt;
		public DialogueTagBankSO TagBank => tagBank;
		public FileManagerSO FileManager => fileManager;
		public InputManagerSO InputManager => inputManager;
		public DialogueReader Reader => dialogueReader;
		public DialogueTagManager TagManager => tagManager;
		public ScriptVariableManager VariableManager => variableManager;
		public AudioManager Audio => audioManager;
		public VisualGroupManager Visuals => visualManager;
		public DialogueReadMode ReadMode => readMode;

		// TODO remove and manage dynamically
		[SerializeField] HistoryState historyState;

		void Start()
		{
			SubscribeEvents();

			tagManager = new(this);
			variableManager = new();
			dialogueReader = new(this);
		}

		void OnDestroy()
		{
			UnsubscribeEvents();
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				//StartCoroutine(RunTest());

				StartCoroutine(ReadDialogueProcess(dialogueFileName));
			}

			// TODO capture and load history states dynamically
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				Debug.Log("Capturing history state...");
				historyState = historyManager.Capture();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) && historyState != null)
			{
				Debug.Log("Loading history state...");
				historyManager.Load(historyState);
			}
		}

		public Coroutine Say(string speakerName, List<string> lines)
		{
			DialogueFile dialogueFile = new DialogueFile(this, speakerName, lines);
			return dialogueReader.StartReading();
		}

		public Coroutine LoadDialogue(string dialoguePath)
		{
			if (dialogueLoadProcess != null)
			{
				StopCoroutine(dialogueLoadProcess);
				dialogueLoadProcess = null;
			}

			dialogueLoadProcess = StartCoroutine(LoadDialogueProcess(dialoguePath));
			return dialogueLoadProcess;
		}

		IEnumerator LoadDialogueProcess(string dialoguePath)
		{
			DialogueFile dialogueFile = new(this, dialoguePath);
			yield return dialogueFile.Load();
			dialogueLoadProcess = null;
		}

		IEnumerator ReadDialogueProcess(string dialoguePath)
		{
			yield return LoadDialogue(dialoguePath);
			yield return dialogueReader.StartReading();
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
		}

		void UnsubscribeEvents()
		{
			inputManager.OnAdvance -= HandleOnAdvanceEvent;
			inputManager.OnAuto -= HandleOnAutoEvent;
			inputManager.OnSkip -= HandleOnSkipEvent;
			inputManager.OnSkipHold -= HandleOnSkipHoldEvent;
			inputManager.OnSkipHoldEnd -= HandleOnSkipHoldEndEvent;
		}

		// TODO Remove test function
		IEnumerator RunTest()
		{
			yield return null;
		}
	}
}
