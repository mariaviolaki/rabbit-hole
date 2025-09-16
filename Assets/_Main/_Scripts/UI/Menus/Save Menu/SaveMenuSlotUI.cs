using Game;
using IO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SaveMenuSlotUI : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] RectTransform dataRoot;
		[SerializeField] TextMeshProUGUI noDataText;
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] Image slotImage;
		[SerializeField] TextMeshProUGUI sceneText;
		[SerializeField] TextMeshProUGUI dateText;
		[SerializeField] Button slotButton;
		
		SaveMenuUI saveMenu;
		SaveMenuMode saveMode;
		SaveSlot slotData;
		int pageNumber = 0;
		int siblingNumber = 0;

		public int GetSlotNumber() => (pageNumber - 1) * saveMenu.SlotsPerPage + siblingNumber;

		void OnEnable()
		{
			slotButton.onClick.AddListener(SaveOrLoad);
		}

		void OnDisable()
		{
			slotButton.onClick.RemoveAllListeners();
		}

		public void Initialize(SaveMenuUI saveMenu, int siblingNumber)
		{
			this.saveMenu = saveMenu;
			this.siblingNumber = siblingNumber;
			ClearSlotData();
		}

		bool SetData() => SetData(saveMode, pageNumber);
		public bool SetData(SaveMenuMode saveMode, int pageNumber)
		{
			this.saveMode = saveMode;
			this.pageNumber = pageNumber;

			// Check if this a valid slot
			int slotNumber = GetSlotNumber();
			if (slotNumber <= 0)
			{
				ClearSlotData();
				return false;
			}

			// Check if this slot needs to be populated with data
			noDataText.text = $"Data {slotNumber}";
			if (!saveMenu.SaveFiles.HasSave(slotNumber))
			{
				ClearSlotData();
				return false;
			}

			// Check if the saved data is valid
			slotData = saveMenu.SaveFiles.LoadSlot(slotNumber);
			if (slotData == null)
			{
				ClearSlotData();
				return false;
			}

			titleText.text = $"Data {slotData.slotNumber}";
			sceneText.text = slotData.sceneTitle;
			dateText.text = new DateTime(slotData.dateTicks).ToString("dd/MM/yy HH:mm");
			SetSlotImage();
			
			if (!dataRoot.gameObject.activeInHierarchy)
				dataRoot.gameObject.SetActive(true);

			return true;
		}

		void SaveOrLoad()
		{
			if (saveMenu.IsTransitioning) return;

			int slotNumber = GetSlotNumber();

			if (saveMode == SaveMenuMode.Save)
				SaveGame(slotNumber);
			else if (saveMode == SaveMenuMode.Load)
				LoadGame(slotNumber);
		}

		void SaveGame(int slotNumber)
		{
			if (saveMenu.Game.Scenes.CurrentScene != GameScene.VisualNovel) return;

			saveMenu.Game.VN.Saving.SaveSlot(slotNumber);
			SetData();
		}

		void LoadGame(int slotNumber)
		{
			if (slotData == null) return;

			saveMenu.Game.LoadGame(slotNumber);
			saveMenu.Menus.CloseMenu();
		}

		void SetSlotImage()
		{
			if (vnOptions.IO.UseSlotScreenshots && slotData.screenshot != null)
			{
				Rect screenshotRect = new(0, 0, slotData.screenshot.width, slotData.screenshot.height);
				slotImage.sprite = Sprite.Create(slotData.screenshot, screenshotRect, new Vector2(0.5f, 0.5f));
			}
			else if (!vnOptions.IO.UseSlotScreenshots)
			{
				slotImage.sprite = saveMenu.GetSlotPortrait(slotData.route);
			}
			else
			{
				slotImage.sprite = null;
			}
		}

		void ClearSlotData()
		{
			slotData = null;
			if (dataRoot.gameObject.activeInHierarchy)
				dataRoot.gameObject.SetActive(false);
		}
	}
}
