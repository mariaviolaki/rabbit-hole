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

			SaveFileManager saveFileManager = new(null);
			DialogueTreeBuilder treeBuilder = new();
			DialogueTreeMap treeLookup = new();

			Directory.CreateDirectory(FilePaths.DialogueFileDirectory);

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
	}
}
