using GameIO;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableUI : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] InputManagerSO inputManager;
	[SerializeField] InputActionType inputType;

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (inputType)
		{
			case InputActionType.Advance:
				inputManager.OnAdvance?.Invoke();
				break;
			case InputActionType.Auto:
				inputManager.OnAuto?.Invoke();
				break;
			case InputActionType.Skip:
				inputManager.OnSkip?.Invoke();
				break;
		}
	}
}
