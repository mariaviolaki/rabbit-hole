using System.Collections;

namespace UI
{
	public class SideMenuUI : SlidingMenuBaseUI
	{
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
