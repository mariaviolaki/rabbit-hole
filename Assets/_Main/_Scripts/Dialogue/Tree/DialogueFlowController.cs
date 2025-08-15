using IO;
using System.Collections;
using UnityEngine;

namespace Dialogue
{
	public class DialogueFlowController
	{
		const string StartSectionName = "Main";

		readonly DialogueManager dialogueManager;
		readonly FileManagerSO fileManager;
		readonly SaveFileManager saveFileManager;
		readonly GameManager gameManager;
		readonly GameOptionsSO gameOptions;
		readonly DialogueSpeakerHandler dialogueSpeakerHandler;
		readonly DialogueTextHandler dialogueTextHandler;

		DialogueTreeMap dialogueMap;
		DialogueTree currentTree;
		NodeBase currentNode;
		string currentSectionName;
		bool isRunning;
		bool isSkipping;
		bool isJumpingToSection;

		public GameManager Game => gameManager;
		public DialogueManager Dialogue => dialogueManager;
		public DialogueSpeakerHandler DialogueSpeaker => dialogueSpeakerHandler;
		public DialogueTextHandler DialogueText => dialogueTextHandler;
		public NodeBase CurrentNode => currentNode;
		public string CurrentSectionName => currentSectionName;
		public int CurrentNodeId => currentNode == null ? -1 : currentNode.TreeNode.Id;
		public bool IsRunning => isRunning;
		public bool IsSkipping => isSkipping;

		public DialogueFlowController(GameManager gameManager, DialogueManager dialogueManager)
		{
			this.gameManager = gameManager;
			this.dialogueManager = dialogueManager;
			saveFileManager = gameManager.SaveManager;
			fileManager = dialogueManager.FileManager;
			gameOptions = gameManager.Options;
			dialogueSpeakerHandler = new(dialogueManager, gameManager.Options);
			dialogueTextHandler = new(gameManager, dialogueManager, this);

			isRunning = false;
			isSkipping = false;
			isJumpingToSection = false;

			dialogueManager.OnChangeReadMode += UpdateReadMode;

			dialogueManager.StartCoroutine(InitializeDialogue());
		}

		void UpdateReadMode(DialogueReadMode readMode)
		{
			isSkipping = readMode == DialogueReadMode.Skip;
		}

		public void Dispose()
		{
			dialogueTextHandler.Dispose();
			dialogueManager.OnChangeReadMode -= UpdateReadMode;
		}

		public void StartDialogue()
		{
			isRunning = true;

			if (currentNode != null && !currentNode.IsExecuting)
				currentNode.StartExecution();
		}

		public void StopDialogue()
		{
			if (currentNode != null)
			{
				currentNode.SpeedUpExecution();
				currentNode.CancelExecution();
			}

			isRunning = false;
		}

		// Called by the tree nodes when they finish execution
		public void ProceedToNode(int nodeId)
		{
			if (isJumpingToSection || gameManager.History.IsUpdatingHistory)
			{
				currentNode = null;
				return;
			}

			DialogueTreeNode treeNode = currentTree.GetNode(nodeId);

			if (treeNode == null)
				currentNode = null;
			else
				SetCurrentNode(treeNode);
		}
		
		public IEnumerator JumpToSection(string sectionName, int nodeId = -1)
		{
			if (dialogueMap == null) yield break;
			isJumpingToSection = true;

			// Don't proceed to the next section while the current node is still executing
			while (currentNode != null && currentNode.TreeNode.Type != DialogueNodeType.Jump)
			{
				StopDialogue();
				yield return null;
			}

			// Check if this section is defined inside any tree listed in the map
			string treeName = dialogueMap.GetTreeName(sectionName);
			if (treeName == null)
			{
				Debug.LogWarning($"No dialogue tree found for section '{sectionName}'.");
				isJumpingToSection = false;
				yield break;
			}

			// If the new section is inside another tree, load the tree from the new file
			if (currentTree == null || currentTree.Name != treeName)
			{
				yield return LoadNewTree(treeName);
				isJumpingToSection = false;
				if (currentTree == null) yield break;
			}

			currentSectionName = sectionName;

			// Start from the first node inside the new section by default, or from the specified node if provided
			int startNodeId = nodeId == -1 ? currentTree.GetSection(sectionName).MinId : nodeId;
			DialogueTreeNode startTreeNode = currentTree.GetNode(startNodeId);

			isJumpingToSection = false;
			SetCurrentNode(startTreeNode);	
		}

		public void SpeedUpCurrentNode()
		{
			if (!isRunning || currentNode == null) return;

			if (currentNode.IsExecuting)
				currentNode.SpeedUpExecution();
			else if (!currentNode.IsExecuting)
				currentNode.StartExecution();
		}

		public void InterruptSkipDueToBlockingNode()
		{
			if (gameManager.State.SkipMode == DialogueSkipMode.AfterChoices) return;
			isSkipping = false;
		}

		void SetCurrentNode(DialogueTreeNode treeNode)
		{
			switch (treeNode.Type)
			{
				case DialogueNodeType.Jump:
					currentNode = new JumpNode(treeNode, this);
					break;
				case DialogueNodeType.Condition:
					currentNode = new ConditionNode(treeNode, this);
					break;
				case DialogueNodeType.Choice:
					currentNode = new ChoiceNode(treeNode, this);
					break;
				case DialogueNodeType.Command:
					currentNode = new CommandNode(treeNode, this);
					break;
				case DialogueNodeType.Input:
					currentNode = new InputNode(treeNode, this);
					break;
				case DialogueNodeType.Assignment:
					currentNode = new AssignmentNode(treeNode, this);
					break;
				case DialogueNodeType.Dialogue:
					currentNode = new DialogueNode(treeNode, this);
					break;
			}

			if (currentNode != null && isRunning)
				currentNode.StartExecution();
		}

		IEnumerator LoadNewTree(string fileName)
		{
			// Attempt to load the tree found in the map from the dialogue addressables
			string dialogueFilePath = $"{FilePaths.DialogueAddressablesRoot}{fileName}";
			yield return fileManager.LoadDialogue(dialogueFilePath);
			TextAsset dialogueFile = fileManager.GetDialogueFile(dialogueFilePath);
			if (dialogueFile == null)
			{
				Debug.LogWarning($"Unable to find dialogue file: '{dialogueFilePath}'");
				yield break;
			}

			// Build the tree from raw json
			currentTree = saveFileManager.ParseJson<DialogueTree>(dialogueFile.text);
			if (currentTree == null)
			{
				fileManager.UnloadDialogue(dialogueFilePath);
				Debug.LogWarning($"Unable to parse dialogue tree from the file: '{dialogueFilePath}'");
				yield break;
			}

			currentTree.Initialize();
			fileManager.UnloadDialogue(dialogueFilePath);
		}

		IEnumerator InitializeDialogue()
		{
			while (!fileManager.InitializedKeys.TryGetValue(AssetType.Dialogue, out bool value) || !value) yield return null;

			// Load the dialogue map first to be able to find trees from section names
			yield return InitializeDialogueMap();
			if (dialogueMap == null) yield break;

			// Jump to the beginning of the default first section
			yield return JumpToSection(StartSectionName);
		}

		IEnumerator InitializeDialogueMap()
		{
			// Wait until the file manager has loaded all the dialogue paths
			string mapFilePath = $"{FilePaths.DialogueAddressablesRoot}{FilePaths.DialogueMapFileName}";
			yield return fileManager.LoadDialogue(mapFilePath);

			// Load the dialogue map from the dialogue addressables
			TextAsset dialogueMapFile = fileManager.GetDialogueFile(mapFilePath);
			if (dialogueMapFile == null)
			{
				Debug.LogWarning($"Unable to find dialogue map file: '{mapFilePath}'");
				yield break;
			}

			// Build the dialogue map from raw json
			dialogueMap = saveFileManager.ParseJson<DialogueTreeMap>(dialogueMapFile.text);
			if (dialogueMap == null)
			{
				Debug.LogWarning($"Unable to parse dialogue map from the file: '{mapFilePath}'");
				fileManager.UnloadDialogue(mapFilePath);
				yield break;
			}

			dialogueMap.Initialize();
			fileManager.UnloadDialogue(mapFilePath);
		}
	}
}
