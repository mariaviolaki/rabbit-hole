using Gameplay;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Variables;

namespace UI
{
	public class SaveMenuUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] Button backButton;
		[SerializeField] Transform pageContainer;
		[SerializeField] Transform slotContainer;
		[SerializeField] SaveMenuPageUI pageNumberPrefab;
		[SerializeField] Button previousButton;
		[SerializeField] Button nextButton;
		[SerializeField] SpriteAtlas portraitAtlas;
		[SerializeField] MenusUI menus;
		[SerializeField] GameManager gameManager;

		readonly List<SaveMenuPageUI> pages = new();
		readonly List<SaveMenuSlotUI> slots = new();
		int maxPages;
		int currentPage;
		SaveMenuMode saveMode;
		bool isTransitioning = false;

		public event Action OnClose;

		public bool IsTransitioning => isTransitioning;
		public int SlotsPerPage => slotContainer.childCount;
		public GameStateManager StateManager => gameManager.StateManager;
		public SaveFileManager SaveManager => gameManager.SaveManager;
		public MenusUI Menus => menus;

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

		protected override void OnEnable()
		{
			base.OnEnable();
			SubscribeListeners();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			UnsubscribeListeners();
		}

		public IEnumerator Open(SaveMenuMode saveMode, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			base.isImmediateTransition = isImmediate;

			PrepareOpen(saveMode);
			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			fadeSpeed = fadeSpeed <= 0 ? gameOptions.General.SkipTransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
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
					return portraitAtlas.GetSprite(VariableManager.DefaultPlayerName);
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

		void PrepareOpen(SaveMenuMode saveMode)
		{
			this.saveMode = saveMode;
			titleText.text = saveMode.ToString();
			SetPage(gameManager.StateManager.State.SaveMenuPage);

			foreach (SaveMenuSlotUI slot in slots)
			{
				slot.SetData(saveMode, currentPage);
			}
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

			gameManager.StateManager.State.SetSaveMenuPage(currentPage);

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
			maxPages = Mathf.Max(1, Mathf.FloorToInt(gameOptions.IO.SlotCount / SlotsPerPage));
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

		void SubscribeListeners()
		{
			backButton.onClick.AddListener(menus.CloseMenu);
			previousButton.onClick.AddListener(DecrementPage);
			nextButton.onClick.AddListener(IncrementPage);

			foreach (SaveMenuPageUI menuPage in pages)
			{
				menuPage.OnSelect += SetPage;
			}
		}

		void UnsubscribeListeners()
		{
			backButton.onClick.RemoveListener(menus.CloseMenu);
			previousButton.onClick.RemoveListener(DecrementPage);
			nextButton.onClick.RemoveListener(IncrementPage);

			foreach (SaveMenuPageUI menuPage in pages)
			{
				menuPage.OnSelect -= SetPage;
			}
		}
	}
}
