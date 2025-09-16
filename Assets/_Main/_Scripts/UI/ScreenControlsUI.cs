using IO;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ScreenControlsUI : MonoBehaviour
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] Button interactionArea;
		[SerializeField] Button sideMenuButton;

		MenusUI menus;

		void Start()
		{
			menus = FindObjectOfType<MenusUI>();
		}

		void OnEnable()
		{
			sideMenuButton.onClick.AddListener(OpenSideMenu);
			interactionArea.onClick.AddListener(AdvanceDialogue);
		}

		void OnDisable()
		{
			sideMenuButton.onClick.RemoveListener(OpenSideMenu);
			interactionArea.onClick.RemoveListener(AdvanceDialogue);
		}

		void OpenSideMenu()
		{
			menus.OpenMenu(MenuType.SideMenu);
		}

		void AdvanceDialogue()
		{
			inputManager.TriggerClick();
		}
	}
}
