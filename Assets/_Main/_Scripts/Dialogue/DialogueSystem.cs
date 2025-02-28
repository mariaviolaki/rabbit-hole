using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class DialogueSystem : MonoBehaviour
{
	[SerializeField] DialogueUI dialogueUI;
	[SerializeField] InputManagerSO inputManager;
	[SerializeField] FileManagerSO fileManager;
	[SerializeField] AssetLabelReference dialogueLabel;
	[SerializeField][Range(1, 12)] float textSpeed;
	[SerializeField] TextBuilder.TextMode textMode;

	DialogueWriter dialogueWriter;

	Dictionary<string, DialogueFile> dialogueFiles = new Dictionary<string, DialogueFile>();
	string currentDialogueFile;

	public float TextSpeed { get { return textSpeed; } }
	public TextBuilder.TextMode TextMode { get { return textMode; } }

	void Start()
	{
		dialogueWriter = new DialogueWriter(this, dialogueUI);

		fileManager.OnLoadTextFiles += CacheDialogueFiles;
		inputManager.OnAdvance += AdvanceDialogue;

		LoadDialogueFiles();
	}

	void Update()
	{
		// TODO start dialogue using other triggers
		if (Input.GetKeyDown(KeyCode.Return))
		{
			StartDialogue();
		}
		// TODO remove test functionality
		else if (Input.GetKeyDown(KeyCode.T))
		{
			RunTest();
		}
	}

	void LoadDialogueFiles()
	{
		if (dialogueLabel == null) return;

		fileManager.LoadTextFiles(dialogueLabel);
	}

	void StartDialogue()
	{
		if (currentDialogueFile == null) return;

		DialogueFile dialogueFile = dialogueFiles[currentDialogueFile];
		dialogueWriter.StartWriting(dialogueFile.Lines);
	}

	void AdvanceDialogue()
	{
		if (dialogueWriter.IsRunning) return;

		dialogueWriter.IsRunning = true;
	}

	void CacheDialogueFiles(List<TextAsset> textAssets)
	{
		currentDialogueFile = textAssets[0].name;

		foreach (TextAsset textAsset in textAssets)
		{
			dialogueFiles[textAsset.name] = new DialogueFile(textAsset.name, textAsset.text);
		}
	}

	// TODO remove test function
	void RunTest()
	{
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		int nextSceneIndex = (sceneIndex + 1 == SceneManager.sceneCountInBuildSettings) ? 0 : sceneIndex + 1;
		SceneManager.LoadScene(nextSceneIndex);
	}
}
