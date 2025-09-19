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
		public const char DialogueDirectoryDelimiter = '.';
		public const string DialogueFileExtension = ".json";
		public const string DialogueAddressablesRoot = "Dialogue/";
		public static readonly string DialogueFileDirectory = $"Assets/_Main/{DialogueAddressablesRoot}";
		public static readonly string DialogueMapFileName = "DialogueMap";

		public static string DialogueLookupFilePath => Path.Combine(DialogueFileDirectory, $"{DialogueMapFileName}{DialogueFileExtension}");

		// Character CGs
		public const string CGBankName = "CGBankSO";
		public const char CGSeparator = '.';
		public const string CGFileExtension = ".png";
		public const string CGAddressablesRoot = "CGs/";
		public const string CGThumbnailAddressablesRoot = "CG Thumbnails/";
		public static readonly string CGRoot = $"Assets/_Main/Graphics/{CGAddressablesRoot}";
		public static readonly string CGThumbnailsRoot = $"Assets/_Main/Graphics/{CGThumbnailAddressablesRoot}";
		
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
