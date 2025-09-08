using Dialogue;
using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace IO
{
	[CreateAssetMenu(fileName = "InputManager", menuName = "Scriptable Objects/Input Manager")]
	public class InputManagerSO : ScriptableObject, InputActions.IVNActions
	{
		// Triggered by the InputActions asset
		public Action OnSideMenuOpen;
		public Action OnMenuClose;
		public Action OnConfirm;
		public Action OnForward;
		public Action OnDialogueBack;
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
		bool IsDialoguePanelOpen => CurrentMenu != MenuType.None || IsInputPanelOpen || IsChoicePanelOpen;

		public MenuType CurrentMenu { get; set; } = MenuType.None;
		public bool IsInputPanelOpen { get; set; } = false;
		public bool IsChoicePanelOpen { get; set; } = false;

		void OnEnable()
		{
			if (inputActions != null) return;

			inputActions = new InputActions();
			inputActions.VN.SetCallbacks(this);
			inputActions.VN.Enable();
		}

		void InputActions.IVNActions.OnForwardAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnForward?.Invoke();
		}

		void InputActions.IVNActions.OnDialogueBackAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnDialogueBack?.Invoke();
		}

		void InputActions.IVNActions.OnAutoAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnAuto?.Invoke();
		}

		void InputActions.IVNActions.OnSkipAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;

			// Only trigger successful button presses
			if (context.interaction is not PressInteraction || context.phase != InputActionPhase.Performed) return;
			OnSkip?.Invoke();
		}

		void InputActions.IVNActions.OnSkipHoldAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;
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

		void InputActions.IVNActions.OnLogAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.None) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnOpenLog?.Invoke();
		}

		void InputActions.IVNActions.OnScrollAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.Log) return;
			if (context.phase != InputActionPhase.Performed) return;

			Vector2 scrollValue = context.ReadValue<Vector2>();

			if (scrollValue.y > 0.1f)
				OnScroll?.Invoke(1f); // scroll up
			else if (scrollValue.y < -0.1f)
				OnScroll?.Invoke(-1f); // scroll down
		}

		void InputActions.IVNActions.OnMenuNavigationAction(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;

			if (CurrentMenu == MenuType.None)
				OnSideMenuOpen?.Invoke();
			else
				OnMenuClose?.Invoke();
		}

		void InputActions.IVNActions.OnConfirmAction(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;

			if (!IsDialoguePanelOpen)
				OnForward?.Invoke();
			else
				OnConfirm?.Invoke();
		}
	}
}