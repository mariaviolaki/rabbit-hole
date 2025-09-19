using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Visuals;

namespace UI
{
	public class GalleryThumbnailUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] Image cgImage;
		[SerializeField] Color activeColor;
		[SerializeField] Color inactiveColor;

		CharacterCG characterCG;

		public event Action<CharacterCG> OnSelectCG;

		void Awake()
		{
			SetUnlocked(false);
			SetActive(false);
		}

		public void SetCG(GalleryMenuUI galleryMenu, CharacterCG characterCG, Sprite sprite)
		{
			if (characterCG != null)
				galleryMenu.Assets.UnloadImage(characterCG.ImageName, galleryMenu.ThumbnailLabel);

			this.characterCG = characterCG;
			cgImage.sprite = sprite;

			SetUnlocked(characterCG != null);
		}

		void SetUnlocked(bool isUnlocked)
		{
			cgImage.enabled = isUnlocked;
		}

		void SetActive(bool isActive)
		{
			cgImage.color = isActive ? activeColor : inactiveColor;
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (characterCG == null) return;

			OnSelectCG?.Invoke(characterCG);
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			SetActive(true);
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			SetActive(false);
		}
	}
}
