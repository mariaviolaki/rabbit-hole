using System.Collections;
using UnityEngine;

namespace UI
{
	public class SideMenuUI : SlidingMenuBaseUI
	{
		[SerializeField] ConfirmationMenuUI confirmationMenu;

		public MenusUI Menus => menus;
		public ConfirmationMenuUI ConfirmationMenu => confirmationMenu;

		override protected IEnumerator Initialize()
		{
			// Initialize the start position of the side menu
			yield return null;

			moveAnimationSpeedMultiplier = 0.00001f;
			openPosition = slideRoot.anchoredPosition;
			closedPosition = new(openPosition.x + closedPositionOffset, openPosition.y);
			
			yield return base.Initialize();
		}
	}
}
