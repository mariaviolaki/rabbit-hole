using Dialogue;
using Gameplay;
using History;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using VN;

namespace IO
{
	[CreateAssetMenu(fileName = "Save File Manager", menuName = "Scriptable Objects/Save File Manager")]
	public class SaveFileManagerSO : ScriptableObject
	{
		[SerializeField] VNOptionsSO vnOptions;

		const int ScreenshotQuality = 90;
		readonly byte[] EncryptionKeyBytes = Encoding.UTF8.GetBytes("VNSaveKey");

		public bool HasSettingsSave() => File.Exists(FilePaths.SettingsSavePath);
		public bool HasProgressSave() => File.Exists(FilePaths.ProgressSavePath);
		public bool HasAutosave() => File.Exists(FilePaths.AutosavePath);
		public bool HasSave(int slot) => File.Exists(Path.Combine(FilePaths.SlotsDirectory, $"{slot}{FilePaths.SaveFileExtension}"));

		void OnEnable()
		{
			Directory.CreateDirectory(FilePaths.RootDirectory);
			Directory.CreateDirectory(FilePaths.SlotsDirectory);
			Directory.CreateDirectory(FilePaths.PersistentDirectory);
		}

		public bool SavePlayerSettings(PlayerSettings playerSettings) => SavePersistentData(playerSettings, FilePaths.SettingsSavePath);
		public PlayerSettings LoadPlayerSettings() => LoadPersistentState<PlayerSettings>(FilePaths.SettingsSavePath);

		public bool SavePlayerProgress(PlayerProgress playerProgress) => SavePersistentData(playerProgress, FilePaths.ProgressSavePath);
		public PlayerProgress LoadPlayerProgress() => LoadPersistentState<PlayerProgress>(FilePaths.ProgressSavePath);

		public bool SaveSlot(int slot, CharacterRoute route, string sceneTitle, List<HistoryState> historyStates, Texture2D screenshot)
		{
			if (slot < 0)
			{
				Debug.LogWarning($"Invalid slot number '{slot}' entered while saving. Save aborted.");
				return false;
			}
			else if (historyStates.Count == 0)
			{
				Debug.LogWarning($"Unable to create game save for slot '{slot}' because there was no state to save. Save aborted.");
				return false;
			}

			string filePath = Path.Combine(FilePaths.SlotsDirectory, $"{slot}{FilePaths.SaveFileExtension}");
			SaveSlot gameSave = new(slot, DateTime.Now.Ticks, route, sceneTitle, historyStates, screenshot);

			if (vnOptions.IO.UseSlotScreenshots && screenshot != null)
				SaveScreenshot(screenshot, slot);

			return Save(gameSave, filePath);
		}

		public SaveSlot LoadSlot(int slot)
		{
			if (slot < 0)
			{
				Debug.LogWarning($"Invalid slot number '{slot}' entered while loading. Load aborted.");
				return null;
			}

			string filePath = Path.Combine(FilePaths.SlotsDirectory, $"{slot}{FilePaths.SaveFileExtension}");
			SaveSlot gameSave = Load<SaveSlot>(filePath);

			Texture2D screenshot = vnOptions.IO.UseSlotScreenshots ? LoadScreenshot(slot) : null;
			if (gameSave != null && screenshot != null)
				gameSave.screenshot = screenshot;

			return gameSave;
		}

		public bool SaveDialogueLookup(DialogueTreeMap dialogueLookup)
		{
			if (dialogueLookup == null)
			{
				Debug.LogWarning("Unable to save dialogue lookup because it was null. Save aborted.");
				return false;
			}

			return SaveJson(dialogueLookup, FilePaths.DialogueLookupFilePath);
		}

		public bool SaveCompiledDialogue(DialogueTree dialogueTree)
		{
			if (dialogueTree == null)
			{
				Debug.LogWarning("Unable to save compiled dialogue tree because it was null. Save aborted.");
				return false;
			}

			string filePath = Path.Combine(FilePaths.DialogueFileDirectory, $"{dialogueTree.Name}{FilePaths.DialogueFileExtension}");
			return SaveJson(dialogueTree, filePath);
		}

		public T ParseJson<T>(string json) where T : class
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to parse JSON: {e.Message}");
				return null;
			}
		}

		bool SavePersistentData<T>(T persistentData, string persistentFilePath) where T : class
		{
			if (persistentData == null)
			{
				Debug.LogWarning($"Given invalid persistent data while saving. Save aborted.");
				return false;
			}

			return Save(persistentData, persistentFilePath);
		}

		T LoadPersistentState<T>(string persistentFilePath) where T : class
		{
			return Load<T>(persistentFilePath);
		}

		bool Save<T>(T data, string filePath) where T : class
		{
			if (vnOptions.IO.Encrypt)
				return SaveEncrypted(data, filePath);
			else
				return SaveJson(data, filePath);
		}

		T Load<T>(string filePath) where T : class
		{
			if (vnOptions.IO.Encrypt)
				return LoadEncrypted<T>(filePath);
			else
				return LoadJson<T>(filePath);
		}

		bool SaveScreenshot(Texture2D screenshot, int slot)
		{
			if (screenshot == null) return false;

			byte[] screenshotBytes = screenshot.EncodeToJPG(ScreenshotQuality);
			string filePath = Path.Combine(FilePaths.SlotsDirectory, $"{slot}.jpg");

			if (vnOptions.IO.Encrypt)
				screenshotBytes = GetXORBytes(screenshotBytes, EncryptionKeyBytes);

			File.WriteAllBytes(filePath, screenshotBytes);

			return true;
		}

		Texture2D LoadScreenshot(int slot)
		{
			string filePath = Path.Combine(FilePaths.SlotsDirectory, $"{slot}.jpg");
			if (!File.Exists(filePath)) return null;

			byte[] screenshotBytes = File.ReadAllBytes(filePath);

			if (vnOptions.IO.Encrypt)
				screenshotBytes = GetXORBytes(screenshotBytes, EncryptionKeyBytes);

			Texture2D screenshot = new(1, 1);
			screenshot.LoadImage(screenshotBytes);

			return screenshot;
		}

		bool SaveJson<T>(T data, string filePath) where T : class
		{
			try
			{
				string gameSaveJson = JsonUtility.ToJson(data, true);
				File.WriteAllText(filePath, gameSaveJson);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to save json in path '{filePath}': {e.Message}");
				return false;
			}
		}

		T LoadJson<T>(string filePath) where T : class
		{
			try
			{
				string gameSaveJson = File.ReadAllText(filePath);
				return JsonUtility.FromJson<T>(gameSaveJson);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to load json from path '{filePath}': {e.Message}");
				return null;
			}
		}

		bool SaveEncrypted<T>(T data, string filePath) where T : class
		{
			try
			{
				string gameSaveJson = JsonUtility.ToJson(data);
				byte[] gameSaveBytes = Encoding.UTF8.GetBytes(gameSaveJson);
				byte[] encryptedBytes = GetXORBytes(gameSaveBytes, EncryptionKeyBytes);
				File.WriteAllBytes(filePath, encryptedBytes);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to create game save in path '{filePath}': {e.Message}");
				return false;
			}
		}

		T LoadEncrypted<T>(string filePath) where T : class
		{
			try
			{
				byte[] encryptedBytes = File.ReadAllBytes(filePath);
				byte[] decryptedBytes = GetXORBytes(encryptedBytes, EncryptionKeyBytes);
				string gameSaveJson = Encoding.UTF8.GetString(decryptedBytes);
				return JsonUtility.FromJson<T>(gameSaveJson);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to load game save from path '{filePath}': {e.Message}");
				return null;
			}
		}

		byte[] GetXORBytes(byte[] historyStateBytes, byte[] keyBytes)
		{
			byte[] xorBytes = new byte[historyStateBytes.Length];

			for (int i = 0; i < historyStateBytes.Length; i++)
			{
				xorBytes[i] = (byte)(historyStateBytes[i] ^ keyBytes[i % keyBytes.Length]);
			}

			return xorBytes;
		}
	}
}
