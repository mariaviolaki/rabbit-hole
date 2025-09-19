using Gameplay;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class GalleryRouteButtonUI : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] Image routePortrait;
		[SerializeField] Color activeColor;
		[SerializeField] Color inactiveColor;
		[SerializeField] CharacterRoute route;

		public event Action<CharacterRoute> OnSelectRoute;

		public CharacterRoute Route => route;

		void Awake()
		{
			SetActive(false);
		}

		public void SetActive(bool isActive)
		{
			routePortrait.color = isActive ? activeColor : inactiveColor;
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			OnSelectRoute?.Invoke(route);
		}
	}
}
