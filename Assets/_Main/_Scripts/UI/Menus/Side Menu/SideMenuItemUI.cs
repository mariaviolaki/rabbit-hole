using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SideMenuItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] MenuType menuType;
		[SerializeField] CanvasGroup imageContainerCanvasGroup;
		[SerializeField] SideMenuUI sideMenu;

		void SetBackgroundImageVisibility(bool isVisible)
		{
			imageContainerCanvasGroup.alpha = isVisible ? 1f : 0f;
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (menuType == MenuType.Title)
			{
				string title = "Return to the title screen?\nAny unsaved progress will be lost.";
				sideMenu.ConfirmationMenu.Open(title, () => sideMenu.Menus.OpenMenu(MenuType.Title));
			}
			else
			{
				sideMenu.Menus.OpenMenu(menuType);
			}
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
