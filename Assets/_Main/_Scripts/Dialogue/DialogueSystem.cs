using Audio;
using Characters;
using Commands;
using GameIO;
using Visuals;
using History;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Variables;
using Logic;

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

		ScriptTagManager tagManager;
		ScriptVariableManager variableManager;
		LogicSegmentManager logicSegmentManager;
		DialogueReader dialogueReader;
		DialogueReadMode readMode = DialogueReadMode.Forward;
		Coroutine dialogueProcess;

		public GameOptionsSO Options => gameOptions;
		public VisualNovelUI UI => visualNovelUI;
		public CharacterManager Characters => characterManager;
		public CommandManager Commands => commandManager;
		public DialogueContinuePromptUI ContinuePrompt => continuePrompt;
		public DialogueTagBankSO TagBank => tagBank;
		public FileManagerSO FileManager => fileManager;
		public InputManagerSO InputManager => inputManager;
		public DialogueReader Reader => dialogueReader;
		public ScriptTagManager TagManager => tagManager;
		public ScriptVariableManager VariableManager => variableManager;
		public AudioManager Audio => audioManager;
		public VisualGroupManager Visuals => visualManager;
		public LogicSegmentManager Logic => logicSegmentManager;
		public HistoryManager History => historyManager;
		public DialogueReadMode ReadMode => readMode;

		void Start()
		{
			SubscribeEvents();

			tagManager = new(this);
			variableManager = new();
			logicSegmentManager = new(this);
			dialogueReader = new(this);
		}

		void OnDestroy()
		{
			dialogueReader.Dispose();
			logicSegmentManager.Dispose();

			UnsubscribeEvents();
		}

		void Update()
		{
			// TODO start dialogue using other triggers
			if (Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				//StartCoroutine(RunTest());

				StartCoroutine(LoadAndRead(dialogueFileName));
			}
		}

		public Coroutine LoadDialogue(string dialoguePath)
		{
			StopProcess(ref dialogueProcess);
			dialogueProcess = StartCoroutine(LoadDialogueProcess(dialoguePath));
			return dialogueProcess;
		}

		public Coroutine ReadDialogue(DialogueReadMode dialogueReadMode)
		{
			// If no read mode is specified, use the current one
			dialogueReadMode = dialogueReadMode == DialogueReadMode.None ? readMode : dialogueReadMode;

			StopProcess(ref dialogueProcess);
			dialogueProcess = StartCoroutine(ReadDialogueProcess(dialogueReadMode));
			return dialogueProcess;
		}

		public void StopDialogue()
		{
			dialogueReader.StopReading();
			StopProcess(ref dialogueProcess);
		}

		public Coroutine Say(string speakerName, List<string> lines)
		{
			_ = new DialogueFile(this, speakerName, lines);
			return dialogueReader.StartReading(DialogueReadMode.Forward);
		}

		IEnumerator LoadDialogueProcess(string dialoguePath)
		{
			DialogueFile dialogueFile = new(this, dialoguePath);
			yield return dialogueFile.Load();
			dialogueProcess = null;
		}

		IEnumerator ReadDialogueProcess(DialogueReadMode dialogueReadMode)
		{
			yield return dialogueReader.StartReading(dialogueReadMode);
			dialogueProcess = null;
		}

		// TODO Remove test function
		public IEnumerator LoadAndRead(string dialoguePath)
		{
			yield return LoadDialogue(dialoguePath);
			yield return ReadDialogueProcess(DialogueReadMode.Forward);
			dialogueProcess = null;
		}

		// TODO Remove test function
		IEnumerator RunTest()
		{
			yield return null;
		}

		public void ResetReadMode()
		{
			DialogueReadMode lastReadMode = readMode;
			readMode = DialogueReadMode.Forward;
			UpdateReadMode(lastReadMode, readMode);
		}
		
		void HandleOnForwardEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			bool shouldAdvanceDuringAuto = lastReadMode == DialogueReadMode.Auto && !gameOptions.Dialogue.StopAutoOnClick;

			if (lastReadMode == DialogueReadMode.Forward || shouldAdvanceDuringAuto)
				inputManager.OnAdvance?.Invoke();
			else
				readMode = DialogueReadMode.Forward;

			UpdateReadMode(lastReadMode, readMode);
		}

		void HandleOnAutoEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			readMode = (lastReadMode == DialogueReadMode.Auto) ? DialogueReadMode.Forward : DialogueReadMode.Auto;

			UpdateReadMode(lastReadMode, readMode);
		}

		void HandleOnSkipToggleEvent() => HandleOnSkipEvent(InputActionDuration.Toggle);
		void HandleOnSkipHoldEvent() => HandleOnSkipEvent(InputActionDuration.Hold);
		void HandleOnSkipHoldEndEvent() => HandleOnSkipEvent(InputActionDuration.End);
		void HandleOnSkipEvent(InputActionDuration inputDuration)
		{
			DialogueReadMode lastReadMode = readMode;

			if (inputDuration == InputActionDuration.Toggle)
				readMode = (lastReadMode == DialogueReadMode.Skip) ? DialogueReadMode.Forward : DialogueReadMode.Skip;
			else if (inputDuration == InputActionDuration.Hold)
				readMode = DialogueReadMode.Skip;
			else if (inputDuration == InputActionDuration.End)
				readMode = DialogueReadMode.Forward;

			UpdateReadMode(lastReadMode, readMode);
		}

		void UpdateReadMode(DialogueReadMode lastMode, DialogueReadMode newMode)
		{
			if (lastMode == newMode) return;

			dialogueReader.UpdateReadMode(newMode);

			if (newMode == DialogueReadMode.Auto || newMode == DialogueReadMode.Skip)
				readModeIndicator.Show(newMode);
			else if (lastMode == DialogueReadMode.Auto || lastMode == DialogueReadMode.Skip)
				readModeIndicator.Hide();
		}

		void StopProcess(ref Coroutine coroutine)
		{
			if (coroutine == null) return;

			StopCoroutine(coroutine);
			coroutine = null;
		}

		void SubscribeEvents()
		{
			inputManager.OnForward += HandleOnForwardEvent;
			inputManager.OnAuto += HandleOnAutoEvent;
			inputManager.OnSkip += HandleOnSkipToggleEvent;
			inputManager.OnSkipHold += HandleOnSkipHoldEvent;
			inputManager.OnSkipHoldEnd += HandleOnSkipHoldEndEvent;
		}

		void UnsubscribeEvents()
		{
			inputManager.OnForward -= HandleOnForwardEvent;
			inputManager.OnAuto -= HandleOnAutoEvent;
			inputManager.OnSkip -= HandleOnSkipToggleEvent;
			inputManager.OnSkipHold -= HandleOnSkipHoldEvent;
			inputManager.OnSkipHoldEnd -= HandleOnSkipHoldEndEvent;
		}
	}
}
