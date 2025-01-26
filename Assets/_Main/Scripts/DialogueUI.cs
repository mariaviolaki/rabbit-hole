using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DialogueUI : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI dialogueText;

	[SerializeField] FileManagerSO fileManager;
	[SerializeField] DialogueParserSO textParser;
	[SerializeField][Range(1, 12)] float speed;
	[SerializeField] TextBuilder.TextMode textMode;
	[SerializeField] AssetLabelReference dialogueLabel;

	TextBuilder textBuilder;

	string currentDialogueFile;
	Dictionary<string, string> dialogueFiles = new Dictionary<string, string>();
	List<string> dialogueStrings = new List<string>();

    void Awake()
    {
		textBuilder = new TextBuilder(dialogueText);

		fileManager.OnLoadTextFiles += CacheDialogueFiles;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("Write");
			if (dialogueStrings.Count == 0) return;
			string text = dialogueStrings[Random.Range(0, dialogueStrings.Count - 1)];
			textBuilder.Speed = speed;
			textBuilder.Write(text, textMode);
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			Debug.Log("Append");
			if (dialogueStrings.Count == 0) return;
			string text = dialogueStrings[Random.Range(0, dialogueStrings.Count - 1)];
			textBuilder.Speed = speed;
			textBuilder.Append(text, textMode);
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			Debug.Log("Stop");
			textBuilder.Stop();
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			Debug.Log("Load");
			fileManager.LoadTextFiles(dialogueLabel);
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			Debug.Log("Read");
			ReadDialogueFile();
		}
	}

	void CacheDialogueFiles(List<TextAsset> textAssets)
	{
		currentDialogueFile = textAssets[0].name;

		foreach (TextAsset textAsset in textAssets)
		{
			dialogueFiles[textAsset.name] = textAsset.text;
		}
	}

	void ReadDialogueFile()
	{
		if (currentDialogueFile == null) return;

		dialogueStrings.Clear();
		string fileContents = dialogueFiles[currentDialogueFile];

		// Read the dialogue file contents line by line
		using (StringReader sr = new StringReader(fileContents))
		{
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					dialogueStrings.Add(line);

					Debug.Log($"Parsing: {line}");
					DialogueLine dialogueLine = textParser.Parse(line);
					dialogueLine.Print();
				}
			}
		}
	}
}
