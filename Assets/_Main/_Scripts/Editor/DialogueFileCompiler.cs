using Dialogue;
using IO;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Preprocessing
{
	public static class DialogueFileCompiler
	{
		[MenuItem("Tools/Compile Dialogue Files")]
		public static void CompileDialogueFiles()
		{
			const char DialogueDirectoryDelimiter = '.';

			SaveFileManagerSO saveFileManager = AssetDatabase.LoadAssetAtPath<SaveFileManagerSO>(FilePaths.SaveFileManagerPath);
			DialogueTreeBuilder treeBuilder = new();
			DialogueTreeMap treeLookup = new();

			Directory.CreateDirectory(FilePaths.DialogueFileDirectory);
			DeleteOldFiles();

			string[] filePaths = Directory.GetFiles(FilePaths.DialogueSourcePath, $"*{FilePaths.DialogueSourceExtension}", SearchOption.AllDirectories);
			foreach (string filePath in filePaths)
			{
				string fileName = Path.ChangeExtension(filePath, null).Replace(FilePaths.DialogueSourcePath, "")
					.Replace('\\', DialogueDirectoryDelimiter).Replace('/', DialogueDirectoryDelimiter);
				string[] rawLines = File.ReadAllLines(filePath);

				DialogueTree dialogueTree = treeBuilder.GetDialogueTree(fileName, rawLines);
				foreach (DialogueTreeScene section in dialogueTree.Scenes)
				{
					treeLookup.SceneNames.Add(section.Name);
					treeLookup.TreeNames.Add(dialogueTree.Name);
					treeLookup.SceneTitles.Add(section.Title);
				}

				saveFileManager.SaveCompiledDialogue(dialogueTree);
			}

			saveFileManager.SaveDialogueLookup(treeLookup);

			Debug.Log("<color=#32CD32>Successfully compiled dialogue files!</color>");
		}

		public static void DeleteOldFiles()
		{
			// Don't delete any directories - the only directory should be the Editor folder which should be preserved
			foreach (string file in Directory.GetFiles(FilePaths.DialogueFileDirectory))
			{
				// Preserve only the Editor meta file
				if (file == FilePaths.DialogueSourceMetaPath) continue;

				File.Delete(file);
			}
		}
	}
}
