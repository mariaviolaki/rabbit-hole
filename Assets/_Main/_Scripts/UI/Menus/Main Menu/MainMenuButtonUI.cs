using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class MainMenuButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] TextMeshProUGUI buttonText;
		[SerializeField] CanvasGroup imageCanvasGroup;
		[SerializeField] Color enabledTextColor;
		[SerializeField] Color disabledTextColor;
		[SerializeField] MainMenuAction action;
		[SerializeField] MainMenuUI mainMenu;

		public event Action OnMainMenuAction;

		void Awake()
		{
			imageCanvasGroup.alpha = 0;
		}

		void Start()
		{
			if (mainMenu.IsActionAvailable(action))
				buttonText.color = enabledTextColor;
			else
				buttonText.color = disabledTextColor;
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (!mainMenu.IsActionAvailable(action)) return;

			OnMainMenuAction?.Invoke();
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			if (!mainMenu.IsActionAvailable(action)) return;

			imageCanvasGroup.alpha = 1;
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			if (!mainMenu.IsActionAvailable(action)) return;

			imageCanvasGroup.alpha = 0;
		}
	}
}
