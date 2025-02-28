using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableUI : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] InputManagerSO inputManager;

	public void OnPointerClick(PointerEventData eventData)
	{
		inputManager.OnAdvance?.Invoke();
	}
}
