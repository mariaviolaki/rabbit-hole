using Audio;
using Characters;
using Commands;
using GameIO;
using Visuals;
using History;
using System.Collections;
using UI;
using UnityEngine;
using Variables;
using Logic;
using System;

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
		Coroutine dialogueCoroutine;
		Coroutine readCoroutine;
		Guid currentDialogueId;

		// Keep track of immediate dialogue calls
		bool isShowingImmediateDialogue = false;
		string pendingImmediateDialogue = null;

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
		public Guid CurrentDialogueId => currentDialogueId;

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
				LoadDialogue(dialogueFileName, Guid.NewGuid());
			}
		}

		public void LoadDialogue(string dialoguePath, Guid dialogueId)
		{
			if (dialogueCoroutine != null) return;

			dialogueCoroutine = StartCoroutine(ReplaceDialogue(dialoguePath, dialogueId));
		}

		IEnumerator ReplaceDialogue(string dialoguePath, Guid dialogueId)
		{
			dialogueReader.StopDialogue();
			while (dialogueReader.IsReading) yield return null;
			yield return null;

			// Parse the new dialogue file and update the stack
			DialogueFile dialogueFile = new(fileManager, dialogueReader.Stack, dialoguePath);
			yield return dialogueFile.Load();

			// Change the current dialogue id and wait a frame so that history manage can react to the change
			currentDialogueId = dialogueId;
			yield return null;

			if (readCoroutine != null)
			{
				StopCoroutine(readCoroutine);
				readCoroutine = null;
			}

			// Start reading all the dialogue blocks added to the stack (at least 1 by default)
			dialogueReader.StartDialogue();
			readCoroutine = StartCoroutine(dialogueReader.Read(readMode));

			dialogueCoroutine = null;
		}

		public void ShowImmediateDialogue(string dialogueText)
		{
			if (dialogueText == pendingImmediateDialogue) return;

			pendingImmediateDialogue = dialogueText;
			if (!isShowingImmediateDialogue)
				StartCoroutine(ProcessNextImmediateDialogue());
		}

		IEnumerator ProcessNextImmediateDialogue()
		{
			isShowingImmediateDialogue = true;

			while (pendingImmediateDialogue != null)
			{
				string immediateDialogue = pendingImmediateDialogue;
				pendingImmediateDialogue = null;

				yield return dialogueReader.ReadImmediate(immediateDialogue);
			}

			isShowingImmediateDialogue = false;
		}

		public IEnumerator Wait(float time)
		{
			yield return new WaitForSeconds(time);
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
