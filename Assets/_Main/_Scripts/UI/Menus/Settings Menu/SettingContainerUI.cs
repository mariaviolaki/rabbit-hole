using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Image))]
	public class SettingContainerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] Image backgroundImage;
		[SerializeField] Color backgroundColor;
		[SerializeField] TextMeshProUGUI descriptionText;
		[SerializeField, TextArea(1, 2)] string description;

		void Awake()
		{
			backgroundImage.color = Color.clear;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			backgroundImage.color = backgroundColor;
			descriptionText.text = description;
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			backgroundImage.color = Color.clear;
			descriptionText.text = "";
		}
	}
}
