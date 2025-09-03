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
		[SerializeField] MenusUI menus;

		void Awake()
		{
			sideMenuButton.onClick.AddListener(OpenSideMenu);
			interactionArea.onClick.AddListener(AdvanceDialogue);
		}

		void OnDestroy()
		{
			sideMenuButton.onClick.RemoveListener(OpenSideMenu);
			interactionArea.onClick.RemoveListener(AdvanceDialogue);
		}

		void OpenSideMenu()
		{
			menus.OpenSideMenu();
		}

		void AdvanceDialogue()
		{
			inputManager.OnForward?.Invoke();
		}
	}
}
