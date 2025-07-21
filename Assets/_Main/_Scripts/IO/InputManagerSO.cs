using Logic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace IO
{
	[CreateAssetMenu(fileName = "InputManager", menuName = "Scriptable Objects/Input Manager")]
	public class InputManagerSO : ScriptableObject, InputActions.IVNActions
	{
		// Triggered by the InputActions asset
		public Action OnForward;
		public Action OnBack;
		public Action OnAuto;
		public Action OnSkip;
		public Action OnSkipHold;
		public Action OnSkipHoldEnd;
		public Action OnOpenLog;
		public Action<float> OnScroll;

		// Triggered by UI components
		public Action OnClearInput;
		public Action<string> OnSubmitInput;
		public Action OnClearChoice;
		public Action<DialogueChoice> OnSelectChoice;

		InputActions inputActions;
		bool isHoldPerformed = false;
		bool IsDialoguePanelOpen => IsInputPanelOpen || IsChoicePanelOpen || IsLogPanelOpen;

		public bool IsInputPanelOpen { get; set; } = false;
		public bool IsChoicePanelOpen { get; set; } = false;
		public bool IsLogPanelOpen { get; set; } = false;

		void OnEnable()
		{
			if (inputActions != null) return;

			inputActions = new InputActions();
			inputActions.VN.SetCallbacks(this);
			inputActions.VN.Enable();
		}

		public void OnForwardAction(InputAction.CallbackContext context)
		{
			if (IsDialoguePanelOpen) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnForward?.Invoke();
		}

		public void OnBackAction(InputAction.CallbackContext context)
		{
			if (IsLogPanelOpen) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnBack?.Invoke();
		}

		public void OnAutoAction(InputAction.CallbackContext context)
		{
			if (IsDialoguePanelOpen) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnAuto?.Invoke();
		}

		public void OnSkipAction(InputAction.CallbackContext context)
		{
			if (IsDialoguePanelOpen) return;

			// Only trigger successful button presses
			if (context.interaction is not PressInteraction || context.phase != InputActionPhase.Performed) return;
			OnSkip?.Invoke();
		}

		public void OnSkipHoldAction(InputAction.CallbackContext context)
		{
			if (IsDialoguePanelOpen) return;
			// Only proceed if the player is holding down the button
			if (context.interaction is not HoldInteraction) return;

			if (context.phase == InputActionPhase.Performed)
			{
				isHoldPerformed = true;
				OnSkipHold?.Invoke();
			}
			else if (context.phase == InputActionPhase.Canceled && isHoldPerformed)
			{
				isHoldPerformed = false;
				OnSkipHoldEnd?.Invoke();
			}
		}

		public void OnLogAction(InputAction.CallbackContext context)
		{
			if (IsDialoguePanelOpen) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnOpenLog?.Invoke();
		}

		public void OnScrollAction(InputAction.CallbackContext context)
		{
			if (!IsLogPanelOpen) return;
			if (context.phase != InputActionPhase.Performed) return;

			Vector2 scrollValue = context.ReadValue<Vector2>();

			if (scrollValue.y > 0.1f)
				OnScroll?.Invoke(1f); // scroll up
			else if (scrollValue.y < -0.1f)
				OnScroll?.Invoke(-1f); // scroll down
		}
	}
}