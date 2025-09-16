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
		const float InputDelay = 0.2f;

		// Triggered by the InputActions asset
		public event Action OnSideMenuOpen;
		public event Action OnMenuClose;
		public event Action OnConfirm;
		public event Action OnForward;
		public event Action OnDialogueBack;
		public event Action OnAuto;
		public event Action OnSkip;
		public event Action OnSkipHold;
		public event Action OnSkipHoldEnd;
		public event Action OnOpenLog;
		public event Action<float> OnScroll;

		// Triggered by UI components
		public event Action OnClearInput;
		public event Action<string> OnSubmitInput;
		public event Action OnClearChoice;
		public event Action<DialogueChoice> OnSelectChoice;

		InputActions inputActions;
		float lastInputTime;
		bool isHoldPerformed = false;

		public MenuType CurrentMenu { get; set; } = MenuType.None;
		public bool IsInputPanelOpen { get; set; } = false;
		public bool IsChoicePanelOpen { get; set; } = false;

		void OnEnable()
		{
			if (inputActions != null) return;

			inputActions = new InputActions();
			inputActions.VN.SetCallbacks(this);
			inputActions.VN.Enable();
			lastInputTime = Time.time;
		}

		public void TriggerClick()
		{
			if (CurrentMenu == MenuType.Dialogue && !IsChoicePanelOpen && !IsInputPanelOpen)
				OnForward?.Invoke();
		}

		public void TriggerSubmitInput(string input)
		{
			if (CurrentMenu == MenuType.Dialogue && IsInputPanelOpen)
				OnSubmitInput?.Invoke(input);
		}

		public void TriggerClearInput()
		{
			if (CurrentMenu == MenuType.Dialogue && IsInputPanelOpen)
				OnClearInput?.Invoke();
		}

		public void TriggerSelectChoice(DialogueChoice choice)
		{
			if (CurrentMenu == MenuType.Dialogue && IsChoicePanelOpen)
				OnSelectChoice?.Invoke(choice);
		}

		public void TriggerClearChoice()
		{
			if (CurrentMenu == MenuType.Dialogue && IsChoicePanelOpen)
				OnClearChoice?.Invoke();
		}

		void InputActions.IVNActions.OnForwardAction(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;

			TriggerClick();
		}

		void InputActions.IVNActions.OnConfirmAction(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;

			if (CurrentMenu == MenuType.Dialogue && !IsChoicePanelOpen && !IsInputPanelOpen)
				OnForward?.Invoke();
			else if (CurrentMenu == MenuType.Dialogue && IsInputPanelOpen)
				OnConfirm?.Invoke();
		}

		void InputActions.IVNActions.OnDialogueBackAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.Dialogue) return;
			if (context.phase != InputActionPhase.Performed) return;
			if (IsOnInputCooldown()) return;

			OnDialogueBack?.Invoke();
		}

		void InputActions.IVNActions.OnAutoAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.Dialogue) return;
			if (context.phase != InputActionPhase.Performed) return;

			OnAuto?.Invoke();
		}

		void InputActions.IVNActions.OnSkipAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.Dialogue) return;

			// Only trigger successful button presses
			if (context.interaction is not PressInteraction || context.phase != InputActionPhase.Performed) return;
			OnSkip?.Invoke();
		}

		void InputActions.IVNActions.OnSkipHoldAction(InputAction.CallbackContext context)
		{
			if (CurrentMenu != MenuType.Dialogue) return;
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
			if (CurrentMenu != MenuType.Dialogue) return;
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

			if (CurrentMenu == MenuType.Dialogue)
				OnSideMenuOpen?.Invoke();
			else
				OnMenuClose?.Invoke();
		}

		bool IsOnInputCooldown()
		{
			float currentTime = Time.time;
			if (currentTime < lastInputTime + InputDelay) return true;

			lastInputTime = currentTime;
			return false;
		}
	}
}
