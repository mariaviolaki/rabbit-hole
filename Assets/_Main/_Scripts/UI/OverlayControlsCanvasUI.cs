using IO;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class OverlayControlsCanvasUI : MonoBehaviour
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] Button interactionArea;

		void OnEnable()
		{
			interactionArea.onClick.AddListener(RegisterClick);
		}

		void OnDisable()
		{
			interactionArea.onClick.RemoveListener(RegisterClick);
		}
	
		void RegisterClick()
		{
			inputManager.TriggerClick();
		}
	}
}
