using Audio;
using Characters;
using Commands;
using IO;
using System;
using System.Collections;
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
		[SerializeField] GameManager gameManager;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] AudioManager audioManager;
		[SerializeField] VisualGroupManager visualManager;
		[SerializeField] VisualNovelUI visualNovelUI;
		
		DialogueFlowController flowController;
		ScriptVariableManager variableManager;
		Guid currentDialogueId;
		DialogueReadMode readMode;

		public VisualNovelUI UI => visualNovelUI;
		public CharacterManager Characters => characterManager;
		public CommandManager Commands => commandManager;
		public FileManagerSO FileManager => fileManager;
		public InputManagerSO InputManager => inputManager;
		public ScriptVariableManager VariableManager => variableManager;
		public AudioManager Audio => audioManager;
		public VisualGroupManager Visuals => visualManager;
		public GameState State => gameManager.State;
		public DialogueFlowController FlowController => flowController;
		public Guid CurrentDialogueId => currentDialogueId;
		public DialogueReadMode ReadMode => readMode;

		public event Action<DialogueReadMode> OnChangeReadMode;

		void Awake()
		{
			readMode = DialogueReadMode.Forward;
		}

		void Start()
		{
			SubscribeEvents();

			variableManager = new();
			flowController = new(gameManager, this);
		}

		void OnDestroy()
		{
			UnsubscribeEvents();

			flowController.Dispose();
		}

		public void StartDialogue()
		{
			flowController.StartDialogue();
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

		void HandleOnForwardEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			bool shouldAdvanceDuringAuto = lastReadMode == DialogueReadMode.Auto && !gameOptions.Dialogue.StopAutoOnClick;

			readMode = DialogueReadMode.Forward;

			UpdateReadMode(readMode);

			if (lastReadMode == DialogueReadMode.Forward || shouldAdvanceDuringAuto)
				flowController.SpeedUpCurrentNode();
		}

		void HandleOnAutoEvent()
		{
			DialogueReadMode lastReadMode = readMode;
			readMode = (lastReadMode == DialogueReadMode.Auto) ? DialogueReadMode.Forward : DialogueReadMode.Auto;

			UpdateReadMode(readMode);

			if (readMode == DialogueReadMode.Auto)
				flowController.SpeedUpCurrentNode();
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
				flowController.SpeedUpCurrentNode();
		}

		void UpdateReadMode(DialogueReadMode newMode)
		{
			if (newMode == DialogueReadMode.Auto || newMode == DialogueReadMode.Skip)
				UI.ReadModeIndicator.Show(newMode);
			else
				UI.ReadModeIndicator.Hide();

			OnChangeReadMode?.Invoke(newMode);
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
