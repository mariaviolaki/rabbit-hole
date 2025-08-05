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

			SaveFileManager saveFileManager = new();
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
				foreach (DialogueTreeSection section in dialogueTree.Sections)
				{
					treeLookup.SectionNames.Add(section.Name);
					treeLookup.TreeNames.Add(dialogueTree.Name);
				}

				saveFileManager.SaveCompiledDialogue(dialogueTree);
			}

			saveFileManager.SaveDialogueLookup(treeLookup);

			Debug.Log("<color=#32CD32>Successfully compiled dialogue files!</color>");
		}
	}
}
