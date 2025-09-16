using Game;
using IO;
using System.Collections;
using UnityEngine;
using VN;

namespace Dialogue
{
	public class DialogueFlowController
	{
		public const string InitSceneName = "Init";
		public const string StartSceneName = "Main";

		readonly SettingsManager settingsManager;
		readonly DialogueManager dialogueManager;
		readonly AssetManagerSO assetManager;
		readonly SaveFileManagerSO saveFileManager;
		readonly VNManager vnManager;
		readonly DialogueSpeakerHandler dialogueSpeakerHandler;
		readonly DialogueTextHandler dialogueTextHandler;

		DialogueTreeMap dialogueMap;
		DialogueTree currentTree;
		NodeBase currentNode;
		string currentSceneName;
		bool isRunning;
		bool isSkipping;
		bool isJumpingToScene;
		bool isInitialized;

		public VNManager VN => vnManager;
		public DialogueManager Dialogue => dialogueManager;
		public DialogueSpeakerHandler DialogueSpeaker => dialogueSpeakerHandler;
		public DialogueTextHandler DialogueText => dialogueTextHandler;
		public NodeBase CurrentNode => currentNode;
		public string CurrentSceneName => currentSceneName;
		public string CurrentSceneTitle => dialogueMap?.GetSceneTitle(currentSceneName);
		public int CurrentNodeId => currentNode == null ? -1 : currentNode.TreeNode.Id;
		public bool IsRunning => isRunning;
		public bool IsSkipping => isSkipping;
		public bool IsInitialized => isInitialized;

		public DialogueFlowController(VNManager vnManager)
		{
			this.vnManager = vnManager;
			this.dialogueManager = vnManager.Dialogue;
			settingsManager = vnManager.Game.Settings;
			saveFileManager = vnManager.Game.SaveFiles;
			assetManager = dialogueManager.VN.Assets;
			dialogueSpeakerHandler = new(dialogueManager, vnManager.Options);
			dialogueTextHandler = new(this);

			isInitialized = false;
			isRunning = false;
			isSkipping = false;
			isJumpingToScene = false;

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

		public void StartDialogue(string sceneName, int nodeNum)
		{
			isRunning = true;
			dialogueTextHandler.UpdateTextBuildMode(DialogueReadMode.Forward);

			sceneName ??= StartSceneName;
			dialogueManager.StartCoroutine(JumpToScene(sceneName, nodeNum));
		}

		public void ContinueDialogue()
		{
			if (!isInitialized) return;

			isRunning = true;
			dialogueTextHandler.UpdateTextBuildMode(DialogueReadMode.Forward);

			if (currentNode != null && !currentNode.IsExecuting)
				currentNode.StartExecution();
		}

		public void StopDialogue()
		{
			if (!isInitialized) return;

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
			if (isJumpingToScene || vnManager.History.IsUpdatingHistory)
			{
				currentNode = null;
				return;
			}

			DialogueTreeNode treeNode = currentTree.GetNode(nodeId);
			if (treeNode != null)
			{
				SetCurrentNode(treeNode);
			}
			else if (currentSceneName == InitSceneName)
			{
				currentNode = null;
				isRunning = false;
				isInitialized = true;
			}
			else
			{
				currentNode = null;
				isRunning = false;
				vnManager.CompleteDialogue();
			}			
		}

		public IEnumerator JumpToInitialization()
		{
			if (dialogueMap == null) yield break;

			yield return JumpToScene(InitSceneName);
		}
		
		public IEnumerator JumpToScene(string sceneName, int nodeId = -1)
		{
			while (!isInitialized && sceneName != InitSceneName) yield return null;

			isJumpingToScene = true;

			// Don't proceed to the next scene while the current node is still executing
			while (currentNode != null && currentNode.TreeNode.Type != DialogueNodeType.Jump)
			{
				StopDialogue();
				yield return null;
			}

			// Check if this scene is defined inside any tree listed in the map
			string treeName = dialogueMap.GetTreeName(sceneName);
			if (treeName == null)
			{
				Debug.LogWarning($"No dialogue tree found for scene '{sceneName}'.");
				isJumpingToScene = false;
				yield break;
			}

			// If the new scene is inside another tree, load the tree from the new file
			if (currentTree == null || currentTree.Name != treeName)
			{
				yield return LoadNewTree(treeName);
				isJumpingToScene = false;
				if (currentTree == null) yield break;
			}

			currentSceneName = sceneName;

			// Start from the first node inside the new scene by default, or from the specified node if provided
			int startNodeId = nodeId == -1 ? currentTree.GetScene(sceneName).MinId : nodeId;
			DialogueTreeNode startTreeNode = currentTree.GetNode(startNodeId);

			isJumpingToScene = false;
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
			if (settingsManager.SkipMode == DialogueSkipMode.AfterChoices) return;
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

			if ((currentNode != null && isRunning) || !isInitialized)
				currentNode.StartExecution();
		}

		IEnumerator LoadNewTree(string fileName)
		{
			// Attempt to load the tree found in the map from the dialogue addressables
			string dialogueFilePath = $"{FilePaths.DialogueAddressablesRoot}{fileName}";
			yield return assetManager.LoadDialogue(dialogueFilePath);
			TextAsset dialogueFile = assetManager.GetDialogueFile(dialogueFilePath);
			if (dialogueFile == null)
			{
				Debug.LogWarning($"Unable to find dialogue file: '{dialogueFilePath}'");
				yield break;
			}

			// Build the tree from raw json
			currentTree = saveFileManager.ParseJson<DialogueTree>(dialogueFile.text);
			if (currentTree == null)
			{
				assetManager.UnloadDialogue(dialogueFilePath);
				Debug.LogWarning($"Unable to parse dialogue tree from the file: '{dialogueFilePath}'");
				yield break;
			}

			currentTree.Initialize();
			assetManager.UnloadDialogue(dialogueFilePath);
		}

		IEnumerator InitializeDialogue()
		{
			while (!assetManager.InitializedKeys.TryGetValue(AssetType.Dialogue, out bool value) || !value) yield return null;

			isRunning = true;
			// Load the dialogue map first to be able to find trees from scene names
			yield return InitializeDialogueMap();
			// Run the initialization scene as defined in the dialogue scripts before moving to any other scene
			yield return JumpToScene(InitSceneName);


		}

		IEnumerator InitializeDialogueMap()
		{
			// Wait until the file manager has loaded all the dialogue paths
			string mapFilePath = $"{FilePaths.DialogueAddressablesRoot}{FilePaths.DialogueMapFileName}";
			yield return assetManager.LoadDialogue(mapFilePath);

			// Load the dialogue map from the dialogue addressables
			TextAsset dialogueMapFile = assetManager.GetDialogueFile(mapFilePath);
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
				assetManager.UnloadDialogue(mapFilePath);
				yield break;
			}

			dialogueMap.Initialize();
			assetManager.UnloadDialogue(mapFilePath);
		}
	}
}
