using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SaveMenuPageUI : MonoBehaviour
	{
		[SerializeField] Button pageButton;
		[SerializeField] Image pageImage;
		[SerializeField] TextMeshProUGUI pageText;
		[SerializeField] Color selectedColor;

		int pageNumber = 0;

		public event Action<int> OnSelect;

		public int PageNumber => pageNumber;

		void OnDestroy()
		{
			pageButton.onClick.RemoveAllListeners();
		}

		public void Initialize(int pageNumber)
		{
			this.pageNumber = pageNumber;
			pageText.text = pageNumber.ToString();
			pageButton.onClick.AddListener(() => OnSelect?.Invoke(pageNumber));
		}

		public void Select()
		{
			pageImage.color = selectedColor;
		}

		public void Deselect()
		{
			pageImage.color = Color.white;
		}
	}
}
