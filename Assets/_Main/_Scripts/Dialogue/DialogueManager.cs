using Characters;
using Commands;
using IO;
using System;
using System.Collections;
using UnityEngine;
using Visuals;
using VN;

namespace Dialogue
{
	public class DialogueManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] VNManager vnManager;
		[SerializeField] CommandManager commandManager;
		[SerializeField] CharacterManager characterManager;
		[SerializeField] VisualGroupManager visualManager;
		
		DialogueFlowController flowController;
		DialogueReadMode readMode;

		public event Action<DialogueReadMode> OnChangeReadMode;

		public VNManager VN => vnManager;
		public CharacterManager Characters => characterManager;
		public CommandManager Commands => commandManager;
		public VisualGroupManager Visuals => visualManager;
		public DialogueFlowController FlowController => flowController;
		public DialogueReadMode ReadMode => readMode;

		void Awake()
		{
			readMode = DialogueReadMode.Forward;
			flowController = new(vnManager);
		}

		void Start()
		{
			SubscribeEvents();
		}

		void OnDestroy()
		{
			UnsubscribeEvents();

			flowController.Dispose();
		}

		public void StartDialogue(string sceneName, int nodeNum)
		{
			flowController.StartDialogue(sceneName, nodeNum);
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
			bool shouldAdvanceDuringAuto = lastReadMode == DialogueReadMode.Auto && !vnOptions.Dialogue.StopAutoOnClick;

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
				StartCoroutine(vnManager.UI.ReadModeIndicator.Show(newMode));
			else
				StartCoroutine(vnManager.UI.ReadModeIndicator.Hide());

			OnChangeReadMode?.Invoke(newMode);
		}

		void SubscribeEvents()
		{
			vnManager.Input.OnForward += HandleOnForwardEvent;
			vnManager.Input.OnAuto += HandleOnAutoEvent;
			vnManager.Input.OnSkip += HandleOnSkipToggleEvent;
			vnManager.Input.OnSkipHold += HandleOnSkipHoldEvent;
			vnManager.Input.OnSkipHoldEnd += HandleOnSkipHoldEndEvent;
		}

		void UnsubscribeEvents()
		{
			vnManager.Input.OnForward -= HandleOnForwardEvent;
			vnManager.Input.OnAuto -= HandleOnAutoEvent;
			vnManager.Input.OnSkip -= HandleOnSkipToggleEvent;
			vnManager.Input.OnSkipHold -= HandleOnSkipHoldEvent;
			vnManager.Input.OnSkipHoldEnd -= HandleOnSkipHoldEndEvent;
		}
	}
}
