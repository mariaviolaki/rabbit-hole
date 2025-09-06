using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SideMenuItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField] MenuType menuType;
		[SerializeField] CanvasGroup imageContainerCanvasGroup;
		[SerializeField] MenusUI menus;

		public void OnPointerClick(PointerEventData eventData)
		{
			menus.OpenMenu(menuType);
		}

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
