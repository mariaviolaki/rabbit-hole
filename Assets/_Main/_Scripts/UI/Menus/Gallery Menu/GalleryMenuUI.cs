using Game;
using Gameplay;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace UI
{
	public class GalleryMenuUI : MenuBaseUI
	{
		[SerializeField] List<GalleryRouteButtonUI> routeButtons;
		[SerializeField] List<GalleryThumbnailUI> thumbnailContainers;
		[SerializeField] GalleryCGUI cgContainer;
		[SerializeField] CGBankSO cgBank;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] AssetLabelReference thumbnailLabel;
		[SerializeField] GameProgressManager gameProgressManager;

		bool isSelectingRoute = false;

		public override bool IsTransitioning => isTransitioning || isSelectingRoute;
		public AssetManagerSO Assets => assetManager;
		public AssetLabelReference ThumbnailLabel => thumbnailLabel;

		override protected bool PrepareOpen(MenuType menuType)
		{
			if (isSelectingRoute) return false;

			cgContainer.gameObject.SetActive(false);
			SelectRoute(routeButtons[0].Route);
			return base.PrepareOpen(menuType);
		}

		protected override void CompleteClose()
		{
			SelectRoute(CharacterRoute.None);

			base.CompleteClose();
		}

		void SelectRoute(CharacterRoute route)
		{
			if (isSelectingRoute && route != CharacterRoute.None) return;

			StartCoroutine(UpdateGalleryRoute(route));
		}

		void SelectCG(CharacterCG characterCG)
		{
			if (IsTransitioning) return;

			cgContainer.SetCG(characterCG);
		}

		IEnumerator UpdateGalleryRoute(CharacterRoute route)
		{
			while (isSelectingRoute) yield return null;
			isSelectingRoute = true;

			List<IEnumerator> loadProcesses = new();
			List<CharacterCG> characterCGs = cgBank.GetBaseCGs(route);

			foreach (CharacterCG cg in characterCGs)
			{
				if (gameProgressManager.HasCG(cg.route, cg.num))
					loadProcesses.Add(assetManager.LoadImage(cg.ImageName, thumbnailLabel));
			}

			yield return Utilities.RunConcurrentProcesses(this, loadProcesses);

			for (int i = 0; i < thumbnailContainers.Count; i++)
			{
				GalleryThumbnailUI thumbnailContainer = thumbnailContainers[i];

				if (i < characterCGs.Count)
				{
					CharacterCG cg = characterCGs[i];
					Sprite sprite = assetManager.GetImage(cg.ImageName, thumbnailLabel);
					if (sprite != null)
						thumbnailContainer.SetCG(this, cg, sprite);
					else
						thumbnailContainer.SetCG(this, null, null);

					thumbnailContainer.gameObject.SetActive(true);
					continue;
				}

				thumbnailContainer.SetCG(this, null, null);
				thumbnailContainer.gameObject.SetActive(false);
			}

			if (route != CharacterRoute.None)
				UpdateRouteButtons(route);

			isSelectingRoute = false;
		}

		void UpdateRouteButtons(CharacterRoute route)
		{
			foreach (GalleryRouteButtonUI routeButton in routeButtons)
			{
				routeButton.SetActive(routeButton.Route == route);
			}
		}

		override protected void SubscribeListeners()
		{
			base.SubscribeListeners();

			foreach (GalleryRouteButtonUI routeButton in routeButtons)
			{
				routeButton.OnSelectRoute += SelectRoute;
			}
			foreach (GalleryThumbnailUI thumbnailContainer in thumbnailContainers)
			{
				thumbnailContainer.OnSelectCG += SelectCG;
			}
		}

		override protected void UnsubscribeListeners()
		{
			base.UnsubscribeListeners();

			foreach (GalleryRouteButtonUI routeButton in routeButtons)
			{
				routeButton.OnSelectRoute -= SelectRoute;
			}
			foreach (GalleryThumbnailUI thumbnailContainer in thumbnailContainers)
			{
				thumbnailContainer.OnSelectCG -= SelectCG;
				thumbnailContainer.SetCG(this, null, null);
				thumbnailContainer.gameObject.SetActive(false);
			}
		}
	}
}
