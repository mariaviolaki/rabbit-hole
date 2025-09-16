using Game;
using Gameplay;
using IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Variables;

namespace UI
{
	public class SaveMenuUI : MenuBaseUI
	{
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] Transform pageContainer;
		[SerializeField] Transform slotContainer;
		[SerializeField] SaveMenuPageUI pageNumberPrefab;
		[SerializeField] Button previousButton;
		[SerializeField] Button nextButton;
		[SerializeField] SpriteAtlas portraitAtlas;
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] GameManager gameManager;

		readonly List<SaveMenuPageUI> pages = new();
		readonly List<SaveMenuSlotUI> slots = new();
		int maxPages;
		int currentPage;
		SaveMenuMode saveMode;

		public int SlotsPerPage => slotContainer.childCount;
		public SaveFileManagerSO SaveFiles => saveFileManager;
		public MenusUI Menus => menus;
		public GameManager Game => gameManager;

		override protected void Awake()
		{
			base.Awake();
			InitializePages();
			InitializeSlots();
		}

		override protected void Start()
		{
			base.Start();
			SetSelectedPage();
		}

		public Sprite GetSlotPortrait(CharacterRoute route)
		{
			switch (route)
			{
				case CharacterRoute.Void:
				case CharacterRoute.Zero:
				case CharacterRoute.Marsh:
					return portraitAtlas.GetSprite(route.ToString());
				case CharacterRoute.Common:
				default:
					return portraitAtlas.GetSprite(DefaultVariables.DefaultPlayerName);
			}
		}

		void SetPage(int pageNumber)
		{
			if (currentPage == pageNumber) return;
			currentPage = pageNumber;

			SetSelectedPage();
		}

		void DecrementPage()
		{
			if (currentPage == 1) return;
			currentPage--;

			SetSelectedPage();
		}

		void IncrementPage()
		{
			if (currentPage == maxPages) return;
			currentPage++;

			SetSelectedPage();
		}

		override protected bool PrepareOpen(MenuType menuType)
		{
			saveMode = menuType == MenuType.Save ? SaveMenuMode.Save : SaveMenuMode.Load;
			titleText.text = saveMode.ToString();
			SetPage(gameManager.Progress.SaveMenuPage);

			foreach (SaveMenuSlotUI slot in slots)
			{
				slot.SetData(saveMode, currentPage);
			}
			
			return base.PrepareOpen(menuType);
		}

		void SetSelectedPage()
		{
			foreach (SaveMenuPageUI menuPage in pages)
			{
				if (menuPage.PageNumber == currentPage)
					menuPage.Select();
				else
					menuPage.Deselect();
			}

			previousButton.interactable = currentPage > 1;
			nextButton.interactable = currentPage < maxPages;

			gameManager.Progress.SetSaveMenuPage(currentPage);

			SetPageSlots();
		}

		void SetPageSlots()
		{
			foreach (SaveMenuSlotUI slot in slots)
			{
				slot.SetData(saveMode, currentPage);
			}
		}

		void InitializePages()
		{
			maxPages = Mathf.Max(1, Mathf.FloorToInt(vnOptions.IO.SlotCount / SlotsPerPage));
			currentPage = 1;

			for (int i = 1; i <= maxPages; i++)
			{
				SaveMenuPageUI menuPage = Instantiate(pageNumberPrefab, pageContainer);
				menuPage.Initialize(i);
				pages.Add(menuPage);
			}
		}

		void InitializeSlots()
		{
			int slotNumber = 0;
			foreach (Transform slotTransform in slotContainer)
			{
				SaveMenuSlotUI slot = slotTransform.GetComponent<SaveMenuSlotUI>();
				slot.Initialize(this, ++slotNumber);

				slots.Add(slot);
			}
		}

		override protected void SubscribeListeners()
		{
			base.SubscribeListeners();
			previousButton.onClick.AddListener(DecrementPage);
			nextButton.onClick.AddListener(IncrementPage);

			foreach (SaveMenuPageUI menuPage in pages)
			{
				menuPage.OnSelect += SetPage;
			}
		}

		override protected void UnsubscribeListeners()
		{
			base.UnsubscribeListeners();
			previousButton.onClick.RemoveListener(DecrementPage);
			nextButton.onClick.RemoveListener(IncrementPage);

			foreach (SaveMenuPageUI menuPage in pages)
			{
				menuPage.OnSelect -= SetPage;
			}
		}
	}
}
