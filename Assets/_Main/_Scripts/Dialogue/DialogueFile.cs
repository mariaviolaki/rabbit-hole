using IO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dialogue
{
	public class DialogueFile
	{
		public const string CommentLineDelimiter = "//";
		const string RootPath = "Dialogue/";

		readonly FileManagerSO fileManager;
		readonly DialogueStack dialogueStack;
		readonly string addressablePath;
		readonly List<string> lines = new();
		string name;

		public string FileName => name;
		public List<string> Lines => lines;

		// Used to load files in addressables using the file manager
		public DialogueFile(string addressablePath, FileManagerSO fileManager, DialogueStack dialogueStack)
		{
			addressablePath = GetNormalizedPath(addressablePath);

			this.fileManager = fileManager;
			this.dialogueStack = dialogueStack;
			this.addressablePath = addressablePath;
		}

		// Creates a custom dialogue to read based on the given lines
		public DialogueFile(List<string> lines, FileManagerSO fileManager, DialogueStack dialogueStack, string speaker)
		{
			this.fileManager = fileManager;
			this.dialogueStack = dialogueStack;

			CreateCustomDialogue(speaker, lines);
		}

		public IEnumerator Load()
		{
			if (string.IsNullOrWhiteSpace(addressablePath)) yield break;

			yield return Parse();

			dialogueStack.Clear();
			dialogueStack.AddBlock(addressablePath, lines);
		}

		public IEnumerator Parse()
		{
			if (string.IsNullOrWhiteSpace(addressablePath)) yield break;

			yield return fileManager.LoadDialogue(addressablePath);
			TextAsset dialogueAsset = fileManager.GetDialogueFile(addressablePath);
			if (dialogueAsset == null) yield break;

			name = dialogueAsset.name;
			ParseLines(dialogueAsset);
		}

		void ParseLines(TextAsset dialogueAsset)
		{
			using StringReader sr = new StringReader(dialogueAsset.text);
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith(CommentLineDelimiter)) continue;

				lines.Add(line);
			}
		}

		void CreateCustomDialogue(string speaker, List<string> lines)
		{
			// Add a custom speaker if one is provided
			if (speaker != null)
			{
				for (int i = 0; i < lines.Count; i++)
				{
					lines[i] = $"{speaker} \"{lines[i]}\"";
				}
			}

			dialogueStack.Clear();
			dialogueStack.AddBlock(null, lines);
		}

		string GetNormalizedPath(string path)
		{
			string normalizedPath = path.Trim().Trim('/');
			if (normalizedPath.StartsWith(RootPath)) return normalizedPath;

			return $"{RootPath}{normalizedPath}";
		}
	}
}
