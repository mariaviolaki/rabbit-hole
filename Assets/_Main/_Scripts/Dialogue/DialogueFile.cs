using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dialogue
{
	public class DialogueFile
	{
		const string RootPath = "Dialogue/";
		const string CommentLineDelimiter = "//";

		readonly DialogueSystem dialogueSystem;
		readonly string addressablePath;
		readonly List<string> lines = new();
		string name;

		public string FileName => name;
		public List<string> Lines => lines;

		// Used to load files in addressables using the file manager
		public DialogueFile(DialogueSystem dialogueSystem, string addressablePath)
		{
			addressablePath = GetNormalizedPath(addressablePath);

			this.dialogueSystem = dialogueSystem;
			this.addressablePath = addressablePath;
		}

		// Creates a custom dialogue to read based on the given lines
		public DialogueFile(DialogueSystem dialogueSystem, string speaker, List<string> lines)
		{
			this.dialogueSystem = dialogueSystem;
			CreateCustomDialogue(speaker, lines);
		}

		public IEnumerator Load()
		{
			if (string.IsNullOrWhiteSpace(addressablePath)) yield break;

			yield return dialogueSystem.FileManager.LoadDialogue(addressablePath);
			TextAsset dialogueAsset = dialogueSystem.FileManager.GetDialogueFile(addressablePath);
			if (dialogueAsset == null) yield break;

			name = dialogueAsset.name;
			ParseLines(dialogueAsset);

			dialogueSystem.Reader.Stack.Clear();
			dialogueSystem.Reader.Stack.AddBlock(addressablePath, lines);
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

			dialogueSystem.Reader.Stack.Clear();
			dialogueSystem.Reader.Stack.AddBlock(null, lines);
		}

		string GetNormalizedPath(string path)
		{
			string normalizedPath = path.Trim().Trim('/');
			if (normalizedPath.StartsWith(RootPath)) return normalizedPath;

			return $"{RootPath}{normalizedPath}";
		}
	}
}
