using Audio;
using Characters;
using Commands;
using IO;
using History;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Variables;
using Visuals;

namespace Dialogue
{
	public class DialogueManager : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] DialogueTagBankSO tagBank;
		[SerializeField] GameManager gameManager;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] AudioManager audioManager;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] VisualGroupManager visualManager;
		[SerializeField] VisualNovelUI visualNovelUI;
		[SerializeField] DialogueContinuePromptUI continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;

		ScriptTagManager tagManager;
		ScriptVariableManager variableManager;
		LogicSegmentManager logicSegmentManager;
		DialogueReader dialogueReader;
		Coroutine dialogueCoroutine;
		Coroutine readCoroutine;
		Guid currentDialogueId;
		DialogueReadMode readMode;

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
		public GameState State => gameManager.State;
		public Guid CurrentDialogueId => currentDialogueId;
		public DialogueReadMode ReadMode { get { return readMode; } }

		void Awake()
		{
			readMode = DialogueReadMode.Forward;
		}

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
			logicSegmentManager.Dispose();

			UnsubscribeEvents();
		}

		public void LoadDialogue(string dialoguePath)
		{
			if (dialogueCoroutine != null) return;

			dialogueCoroutine = StartCoroutine(ReplaceDialogue(dialoguePath));
		}

		public void LoadDialogueWithProgress(string dialoguePath, HistoryState historyState)
		{
			if (dialogueCoroutine != null || historyState == null) return;

			dialogueCoroutine = StartCoroutine(ReplaceDialogueWithProgress(dialoguePath, historyState));
		}

		IEnumerator ReplaceDialogue(string dialoguePath)
		{
			dialogueReader.IsRunning = false;
			while (dialogueReader.IsReading) yield return null;

			if (readCoroutine != null)
			{
				StopCoroutine(readCoroutine);
				readCoroutine = null;
			}

			// Parse the new dialogue file and update the stack
			DialogueFile dialogueFile = new(dialoguePath, fileManager, dialogueReader.Stack);
			yield return dialogueFile.Load();

			// Start reading all the dialogue blocks added to the stack (at least 1 by default)
			readCoroutine = StartCoroutine(dialogueReader.Read(readMode));
			dialogueCoroutine = null;
		}

		IEnumerator ReplaceDialogueWithProgress(string dialoguePath, HistoryState historyState)
		{
			dialogueReader.IsRunning = false;
			while (dialogueReader.IsReading) yield return null;

			if (readCoroutine != null)
			{
				StopCoroutine(readCoroutine);
				readCoroutine = null;
			}

			// Parse the new dialogue file and update the stack
			DialogueFile dialogueFile = new(dialoguePath, fileManager, dialogueReader.Stack);
			yield return dialogueFile.Load();

			// Move the progress of the main block to the right point in the history state
			DialogueBlock mainBlock = dialogueReader.Stack.GetBlock();
			if (mainBlock == null) yield break;

			historyManager.ResetHistoryProgress(historyState);
			//RestoreDialogueProgress(mainBlock, historyDialogueBlocks);
			dialogueReader.LineReader.UpdateTextBuildMode(DialogueReadMode.Forward);

			// Start reading all the dialogue blocks added to the stack (at least 1 by default)
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

		public void SetReadMode(DialogueReadMode readMode)
		{
			this.readMode = readMode;
			UpdateReadMode(readMode);
		}

		void RestoreDialogueProgress(DialogueBlock mainBlock, List<HistoryDialogueBlock> historyDialogueBlocks)
		{
			HistoryDialogueBlock mainHistoryBlock = historyDialogueBlocks.Last();
			mainBlock.LoadProgress(mainHistoryBlock.progress, mainHistoryBlock.fileStartIndex, mainHistoryBlock.fileEndIndex);

			for (int i = historyDialogueBlocks.Count - 2; i >= 0; i--)
			{
				HistoryDialogueBlock historyBlock = historyDialogueBlocks[i];

				// Get a subset of the lines from the main block based on the history block's start and end indices
				List<string> lines = mainBlock.Lines
					.Skip(historyBlock.fileStartIndex)
					.Take(historyBlock.fileEndIndex - historyBlock.fileStartIndex + 1)
					.ToList();

				// Add any subsequent nested blocks back to the stack
				dialogueReader.Stack.AddBlock(
					historyBlock.filePath, lines, historyBlock.fileStartIndex, historyBlock.fileEndIndex, historyBlock.progress);
			}
		}

		void HandleOnForwardEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			bool shouldAdvanceDuringAuto = lastReadMode == DialogueReadMode.Auto && !gameOptions.Dialogue.StopAutoOnClick;

			readMode = DialogueReadMode.Forward;

			UpdateReadMode(readMode);

			if (lastReadMode == DialogueReadMode.Forward || shouldAdvanceDuringAuto)
				dialogueReader.IsWaitingToAdvance = false;
		}

		void HandleOnAutoEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			readMode = (lastReadMode == DialogueReadMode.Auto) ? DialogueReadMode.Forward : DialogueReadMode.Auto;

			UpdateReadMode(readMode);

			if (readMode == DialogueReadMode.Auto)
				dialogueReader.IsWaitingToAdvance = false;
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

			UpdateReadMode(readMode);

			if (readMode == DialogueReadMode.Skip)
				dialogueReader.IsWaitingToAdvance = false;
		}

		void UpdateReadMode(DialogueReadMode newMode)
		{
			dialogueReader.LineReader.UpdateTextBuildMode(newMode);

			if (newMode == DialogueReadMode.Auto || newMode == DialogueReadMode.Skip)
				readModeIndicator.Show(newMode);
			else
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
