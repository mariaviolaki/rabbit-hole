using IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class ClickableUI : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] InputActionType inputType;

		public void OnPointerClick(PointerEventData eventData)
		{
			switch (inputType)
			{
				case InputActionType.Advance:
					inputManager.OnForward?.Invoke();
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
}
