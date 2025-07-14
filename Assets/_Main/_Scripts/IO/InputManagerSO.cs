using Logic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[CreateAssetMenu(fileName = "InputManager", menuName = "Scriptable Objects/Input Manager")]
public class InputManagerSO : ScriptableObject, InputActions.IVNActions
{
	const float InputDelayFast = 0.2f;
	const float InputDelaySlow = 0.3f;

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

	// Triggered by the Dialogue System
	public Action OnAdvance;

	InputActions inputActions;
	float lastInputTime;
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
		lastInputTime = Time.time;
	}

	public void OnForwardAction(InputAction.CallbackContext context)
	{
		if (IsDialoguePanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;
		if (IsOnInputCooldown(InputDelaySlow)) return;

		OnForward?.Invoke();
	}

	public void OnBackAction(InputAction.CallbackContext context)
	{
		if (IsLogPanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;
		if (IsOnInputCooldown(InputDelaySlow)) return;

		OnBack?.Invoke();
	}

	public void OnAutoAction(InputAction.CallbackContext context)
	{
		if (IsDialoguePanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;
		if (IsOnInputCooldown(InputDelayFast)) return;

		OnAuto?.Invoke();
	}

	public void OnSkipAction(InputAction.CallbackContext context)
	{
		if (IsDialoguePanelOpen) return;
		if (IsOnInputCooldown(InputDelayFast)) return;

		// Only trigger successful button presses
		if (context.interaction is not PressInteraction || context.phase != InputActionPhase.Performed) return;

		OnSkip?.Invoke();
	}

	public void OnSkipHoldAction(InputAction.CallbackContext context)
	{
		if (IsDialoguePanelOpen) return;
		if (IsOnInputCooldown(InputDelayFast)) return;

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
		if (IsOnInputCooldown(InputDelayFast)) return;

		OnOpenLog?.Invoke();
	}

	public void OnScrollAction(InputAction.CallbackContext context)
	{
		if (!IsLogPanelOpen) return;
		if (context.phase != InputActionPhase.Performed) return;
		if (IsOnInputCooldown(InputDelayFast)) return;

		Vector2 scrollValue = context.ReadValue<Vector2>();

		if (scrollValue.y > 0.1f)
			OnScroll?.Invoke(1f); // scroll up
		else if (scrollValue.y < -0.1f)
			OnScroll?.Invoke(-1f); // scroll down
	}

	bool IsOnInputCooldown(float delay)
	{
		float currentTime = Time.time;
		if (currentTime < lastInputTime + delay) return true;

		lastInputTime = currentTime;
		return false;
	}
}
