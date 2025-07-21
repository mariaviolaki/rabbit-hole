using History;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace IO
{
	[System.Serializable]
	public class SaveSlot
	{
		public List<HistoryState> HistoryStates;
	}

	public class SaveFileManager
	{
		public int AutosaveSlot = 0;
		public int MinSaveSlot = 1;

		const string FileExtension = "vns";
		const string SettingsFileName = "Settings";
		const string ProgressFileName = "Progress";
		readonly string RootDirectory;
		readonly string PersistentDirectory;
		readonly string SlotsDirectory;
		readonly string SettingsSavePath;
		readonly string ProgressSavePath;
		readonly string AutosavePath;
		readonly byte[] EncryptionKeyBytes = Encoding.UTF8.GetBytes("VNSaveKey");

		public bool HasSettingsSave() => File.Exists(SettingsSavePath);
		public bool HasProgressSave() => File.Exists(ProgressSavePath);
		public bool HasAutosave() => File.Exists(AutosavePath);

		public SaveFileManager()
		{
			RootDirectory = Path.Combine(Application.persistentDataPath, "Saves");
			SlotsDirectory = Path.Combine(RootDirectory, "Slots");
			PersistentDirectory = Path.Combine(RootDirectory, "Persistent");
			SettingsSavePath = Path.Combine(PersistentDirectory, $"{SettingsFileName}.{FileExtension}");
			ProgressSavePath = Path.Combine(PersistentDirectory, $"{ProgressFileName}.{FileExtension}");
			AutosavePath = Path.Combine(SlotsDirectory, $"{AutosaveSlot}.{FileExtension}");

			Directory.CreateDirectory(RootDirectory);
			Directory.CreateDirectory(SlotsDirectory);
			Directory.CreateDirectory(PersistentDirectory);
		}

		public bool SavePlayerSettings(PlayerSettings playerSettings) => SavePersistentData(playerSettings, SettingsSavePath);
		public PlayerSettings LoadPlayerSettings() => LoadPersistentState<PlayerSettings>(SettingsSavePath);

		public bool SavePlayerProgress(PlayerProgress playerProgress) => SavePersistentData(playerProgress, ProgressSavePath);
		public PlayerProgress LoadPlayerProgress() => LoadPersistentState<PlayerProgress>(ProgressSavePath);

		public bool SaveSlot(int slot, List<HistoryState> historyStates)
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

			string filePath = Path.Combine(SlotsDirectory, $"{slot}.{FileExtension}");
			SaveSlot gameSave = new() { HistoryStates = historyStates };

			return SaveJson(gameSave, filePath);
		}

		public List<HistoryState> LoadSlot(int slot)
		{
			if (slot < 0)
			{
				Debug.LogWarning($"Invalid slot number '{slot}' entered while loading. Load aborted.");
				return null;
			}

			string filePath = Path.Combine(SlotsDirectory, $"{slot}.{FileExtension}");
			SaveSlot gameSave = LoadJson<SaveSlot>(filePath);

			return gameSave?.HistoryStates;
		}

		bool SavePersistentData<T>(T persistentData, string persistentFilePath) where T : class
		{
			if (persistentData == null)
			{
				Debug.LogWarning($"Given invalid persistent data while saving. Save aborted.");
				return false;
			}

			return SaveJson(persistentData, persistentFilePath);
		}

		T LoadPersistentState<T>(string persistentFilePath) where T : class
		{
			return LoadJson<T>(persistentFilePath);
		}

		bool Save<T>(T data, string filePath) where T : class
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

		T Load<T>(string filePath) where T : class
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

		// Creates JSON files for testing
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
				Debug.LogWarning($"Unable to create game save in path '{filePath}': {e.Message}");
				return false;
			}
		}

		// Loads JSON files for testing
		T LoadJson<T>(string filePath) where T : class
		{
			try
			{
				string gameSaveJson = File.ReadAllText(filePath);
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
