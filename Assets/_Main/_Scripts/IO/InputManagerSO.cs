using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputManager", menuName = "Scriptable Objects/Input Manager")]
public class InputManagerSO : ScriptableObject, InputActions.IGameActions
{
	public Action OnAdvance;

	InputActions inputActions;
	
	void OnEnable()
	{
		if (inputActions != null) return;

		inputActions = new InputActions();
		inputActions.Game.SetCallbacks(this);
		inputActions.Game.Enable();
	}

	public void OnAdvanceAction(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Performed) return;

		OnAdvance?.Invoke();
	}
}
