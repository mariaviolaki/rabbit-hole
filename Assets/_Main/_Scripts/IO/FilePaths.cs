using System.IO;
using UnityEngine;

namespace IO
{
	public static class FilePaths
	{
		// Used for Editor preprocessing only
		public const string DialogueSourceExtension = ".txt";
		public const string DialogueSourcePath = "Assets/_Main/Dialogue/Editor/";
		public const string DialogueSourceMetaPath = "Assets/_Main/Dialogue/Editor.meta";
		public const string SaveFileManagerPath = "Assets/_Main/Scriptable Objects/Save File Manager.asset";
		
		// Compiled dialogue
		public const string DialogueFileExtension = ".json";
		public const string DialogueAddressablesRoot = "Dialogue/";
		public static readonly string DialogueFileDirectory = $"Assets/_Main/{DialogueAddressablesRoot}";
		public static readonly string DialogueMapFileName = "DialogueMap";

		public static string DialogueLookupFilePath => Path.Combine(DialogueFileDirectory, $"{DialogueMapFileName}{DialogueFileExtension}");

		// Save files
		public const int AutosaveSlot = 0;
		public const int MinSaveSlot = 1;
		public const string SaveFileExtension = ".vns";
		public static readonly string SettingsFileName = $"Settings";
		public static readonly string ProgressFileName = $"Progress";	

		public static string RootDirectory => Application.persistentDataPath;
		public static string PersistentDirectory => Path.Combine(RootDirectory, "Persistent");
		public static string SlotsDirectory => Path.Combine(RootDirectory, "Saves");
		public static string SettingsSavePath => Path.Combine(PersistentDirectory, $"{SettingsFileName}{SaveFileExtension}");
		public static string ProgressSavePath => Path.Combine(PersistentDirectory, $"{ProgressFileName}{SaveFileExtension}");
		public static string AutosavePath => Path.Combine(SlotsDirectory, $"{AutosaveSlot}{SaveFileExtension}");
	}
}
