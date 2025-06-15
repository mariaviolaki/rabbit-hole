using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[CreateAssetMenu(fileName = "InputManager", menuName = "Scriptable Objects/Input Manager")]
public class InputManagerSO : ScriptableObject, InputActions.IGameActions
{
	public Action OnAdvance;
	public Action OnAuto;
	public Action OnSkip;
	public Action OnSkipHold;
	public Action OnSkipHoldEnd;
	public Action OnClearInput;
	public Action<string> OnSubmitInput;

	InputActions inputActions;
	bool isHoldPerformed = false;

	public bool IsInputPanelOpen { get; set; } = false;

	void OnEnable()
	{
		if (inputActions != null) return;

		inputActions = new InputActions();
		inputActions.Game.SetCallbacks(this);
		inputActions.Game.Enable();
	}

	public void OnAdvanceAction(InputAction.CallbackContext context)
	{
		if (IsInputPanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;

		OnAdvance?.Invoke();
	}

	public void OnAutoAction(InputAction.CallbackContext context)
	{
		if (IsInputPanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;

		OnAuto?.Invoke();
	}

	public void OnSkipAction(InputAction.CallbackContext context)
	{
		if (IsInputPanelOpen) return;

		// Only trigger successful button presses
		if (context.interaction is not PressInteraction || context.phase != InputActionPhase.Performed) return;

		OnSkip?.Invoke();
	}

	public void OnSkipHoldAction(InputAction.CallbackContext context)
	{
		if (IsInputPanelOpen) return;

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
