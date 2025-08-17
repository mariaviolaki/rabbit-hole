using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SideMenuItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] MenuType menuType;
		[SerializeField] CanvasGroup imageContainerCanvasGroup;

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetBackgroundImageVisibility(true);
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			SetBackgroundImageVisibility(false);
		}

		void SetBackgroundImageVisibility(bool isVisible)
		{
			imageContainerCanvasGroup.alpha = isVisible ? 1f : 0f;
		}
	}
}
