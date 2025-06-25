using Logic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

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

	// Triggered by UI components
	public Action OnClearInput;
	public Action<string> OnSubmitInput;
	public Action OnClearChoice;
	public Action<DialogueChoice> OnSelectChoice;

	// Triggered by the Dialogue System
	public Action OnAdvance;

	InputActions inputActions;
	bool isHoldPerformed = false;
	bool IsDialoguePanelOpen => IsInputPanelOpen || IsChoicePanelOpen;

	public bool IsInputPanelOpen { get; set; } = false;
	public bool IsChoicePanelOpen { get; set; } = false;

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
}
