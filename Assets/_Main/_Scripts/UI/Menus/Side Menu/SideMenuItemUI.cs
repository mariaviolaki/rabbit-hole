using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SideMenuItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] MenuType menuType;
		[SerializeField] CanvasGroup imageContainerCanvasGroup;
		[SerializeField] MenusUI menus;

		void SetBackgroundImageVisibility(bool isVisible)
		{
			imageContainerCanvasGroup.alpha = isVisible ? 1f : 0f;
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			menus.OpenMenu(menuType);
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			SetBackgroundImageVisibility(true);
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			SetBackgroundImageVisibility(false);
		}
	}
}
