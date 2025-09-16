using Dialogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Preprocessing
{
	public class DialogueTreeBuilder
	{
		// Dialogue lines starting with this delimiter will be ignored
		const string CommentDelimiter = "//";

		// Condition and choice blocks will be grouped with these delimiters
		const string BlockStartDelimiter = "{";
		const string BlockEndDelimiter = "}";

		// Keep track of the start and end of each scene in a dialogue file (cannot be nested)
		static readonly Regex SceneStartRegex = new(@"^\s*label\s+([A-Za-z0-9_]+)(?:\s+""([^""]+)"")?\s*$", RegexOptions.IgnoreCase);
		static readonly Regex SceneEndRegex = new(@"^\s*end\s+([A-Za-z0-9_]+)\s*$", RegexOptions.IgnoreCase);

		// Only allow certain node types in the initialization scene
		static readonly DialogueNodeType[] InitializationNodes = { DialogueNodeType.Command, DialogueNodeType.Assignment };

		// Keep track of the regexes that will be used to match each dialogue line inside a scene
		// Condition nodes are empty containers and cannot be tracked, unlike their branching children
		readonly Dictionary<DialogueNodeType, Regex> NodeTypePatterns = new()
		{
			{ DialogueNodeType.Jump, new Regex(@"^\s*jump\s+([A-Za-z0-9_]+)\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.Input, new Regex(@"^\s*input\s+""((?:[^""\\]|\\.)*)""\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.Choice, new Regex(@"^\s*choice\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.ChoiceBranch, new Regex(@"^\s*-\s*(.+)\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.ConditionBranch, new Regex(@"^\s*(else if|if|else)(?:\s*\((.+)\))?\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.Assignment, new Regex(@"^\s*(\$(?:[A-Za-z0-9_]+\s*\.\s*)*[A-Za-z0-9_]+)\s*(\+=|-=|\*=|/=|=)\s*(.+)\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.Command, new Regex(@"^\s*((?:wait\s*)?[a-zA-Z0-9_]+\s*(?:\.\s*)?[a-zA-Z0-9_]+\s*\(.*\))\s*$", RegexOptions.IgnoreCase) },
			{ DialogueNodeType.Dialogue, new Regex(@"^\s*([A-Za-z0-9_]*)\s*(?:(.*?)\s*)""((?:[^""\\]|\\.)*)""\s*$", RegexOptions.IgnoreCase) },
		};

		int nodeId = -1;

		bool IsValidLine(string line) => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(CommentDelimiter);

		public DialogueTree GetDialogueTree(string fileName, string[] rawLines)
		{
			nodeId = -1;
			List<DialogueTreeScene> scenes = new();

			DialogueTreeScene currentScene = null;

			for (int i = 0; i < rawLines.Length; i++)
			{
				string line = rawLines[i].Trim();
				if (!IsValidLine(line)) continue;

				// Create a new scene for this file
				Match sceneMatch = SceneStartRegex.Match(line);
				if (sceneMatch.Success)
				{
					string sceneName = sceneMatch.Groups[1].Value.Trim();
					if (string.Equals(sceneName, DialogueFlowController.InitSceneName, StringComparison.OrdinalIgnoreCase))
						sceneName = DialogueFlowController.InitSceneName;

					string sceneTitle = sceneMatch.Groups.Count > 2 ? sceneMatch.Groups[2].Value.Trim() : null;

					DialogueTreeScene newScene = new(sceneName, sceneTitle);
					newScene.MinId = nodeId + 1; // this will be set back to -1 if no nodes are found in this scene
					scenes.Add(newScene);
					currentScene = newScene;
					continue;
				}

				// Mark the current scene as complete
				sceneMatch = SceneEndRegex.Match(line);
				if (sceneMatch.Success)
				{
					currentScene = null;
					continue;
				}

				if (currentScene == null) continue;

				i--;
				currentScene.Nodes = currentScene.Name == DialogueFlowController.InitSceneName
					? GetInitializationNodes(rawLines, ref i)
					: GetChildNodes(rawLines, null, ref i);

				// Don't use valid ids if no nodes were found
				if (currentScene.Nodes.Count == 0)
					currentScene.MinId = -1;
				else // Set the max id to the most recent id created
					currentScene.MaxId = nodeId;
			}

			DialogueTree dialogueTree = new() { Name = fileName, Scenes = scenes };
			InitializeNextIds(dialogueTree);

			return dialogueTree;
		}

		List<DialogueTreeNode> GetInitializationNodes(string[] rawLines, ref int lineIndex)
		{
			List<DialogueTreeNode> nodes = new();
			int depth = 0;

			while (++lineIndex < rawLines.Length)
			{
				string line = rawLines[lineIndex].Trim();
				if (line == string.Empty || line.StartsWith(CommentDelimiter)) continue;

				// End initialization scene
				if (SceneEndRegex.Match(line).Success) break;

				// Skip any nested nodes
				if (line.StartsWith(BlockStartDelimiter)) depth++;
				else if (line.StartsWith(BlockEndDelimiter)) depth--;
				if (depth != 0) continue;

				DialogueTreeNode node = GetNodeFromLine(line, null);
				if (node == null || !InitializationNodes.Contains(node.Type)) continue;

				node.SetId(++nodeId);
				nodes.Add(node);
			}

			return nodes;
		}

		List<DialogueTreeNode> GetChildNodes(string[] rawLines, DialogueTreeNode parentNode, ref int lineIndex)
		{
			List<DialogueTreeNode> children = new();

			while (++lineIndex < rawLines.Length)
			{
				string line = rawLines[lineIndex].Trim();
				if (line == string.Empty || line.StartsWith(CommentDelimiter)) continue;

				bool isLastSceneLine = SceneEndRegex.Match(line).Success;
				if (parentNode == null && isLastSceneLine) break;

				bool isBlockStartLine = line.StartsWith(BlockStartDelimiter);
				bool isBlockEndLine = line.StartsWith(BlockEndDelimiter);
				if (isBlockStartLine) continue;

				DialogueTreeNode node = isBlockEndLine ? null : GetNodeFromLine(line, parentNode);
				if (node == null && !isBlockEndLine && !isLastSceneLine) continue;

				// Parse condition blocks
				if (parentNode != null && parentNode.Type == DialogueNodeType.ConditionBranch && isBlockEndLine)
				{
					// End the current branch, but evaluate the closing brace again in case the parent condition needs to end too
					lineIndex--;
					break;
				}
				else if (parentNode != null && parentNode.Type == DialogueNodeType.Condition)
				{
					if (isBlockEndLine)
					{
						// Get the next dialogue line after the closing brace
						int nextLineIndex = lineIndex + 1;
						while (nextLineIndex + 1 < rawLines.Length && !IsValidLine(rawLines[nextLineIndex]))
							nextLineIndex++;

						// If this parent condition has no more branches, return its current children
						if (NodeTypePatterns[DialogueNodeType.ConditionBranch].Match(rawLines[nextLineIndex]).Success) continue;
						else break;
					}
					if (isLastSceneLine || node.Type != DialogueNodeType.ConditionBranch)
					{
						lineIndex--;
						break;
					}

					AddNodeToChildList(node, children);
					node.Children = GetChildNodes(rawLines, node, ref lineIndex);
					continue;
				}
				else if (node != null && node.Type == DialogueNodeType.ConditionBranch)
				{
					// A new condition will be evaluated so add all the next branches inside a parent condition
					DialogueTreeNode conditionParent = new(DialogueNodeType.Condition, parentNode);
					AddNodeToChildList(conditionParent, children);
					lineIndex--;
					conditionParent.Children = GetChildNodes(rawLines, conditionParent, ref lineIndex);
					continue;
				}

				// Parse choice blocks
				if (node != null && node.Type == DialogueNodeType.Choice)
				{
					// Choice parent node starts here, add all the options that follow in the next lines
					AddNodeToChildList(node, children);
					node.Children = GetChildNodes(rawLines, node, ref lineIndex);
					continue;
				}
				else if (parentNode != null && parentNode.Type == DialogueNodeType.Choice)
				{
					// Check whether the choice option branch needs to end here
					if (isBlockEndLine) break;
					if (isLastSceneLine || node.Type != DialogueNodeType.ChoiceBranch)
					{
						lineIndex--;
						break;
					}

					// Add all the nodes under this choice branch
					AddNodeToChildList(node, children);
					node.Children = GetChildNodes(rawLines, node, ref lineIndex);
					continue;
				}
				else if (parentNode != null && parentNode.Type == DialogueNodeType.ChoiceBranch)
				{
					// This choice option ended at the previous line, this new line needs to be evaluated again
					if (isBlockEndLine) break;
				}

				// List this node as a child (default for all single-line nodes)
				if (node != null)
					AddNodeToChildList(node, children);
			}

			if (parentNode != null)
				parentNode.MaxId = nodeId;

			return children;
		}

		void InitializeNextIds(DialogueTree dialogueTree)
		{
			dialogueTree.Initialize();
			foreach (DialogueTreeScene scene in dialogueTree.Scenes)
			{
				InitializeSceneNextIds(dialogueTree, scene);
			}
		}

		DialogueTreeNode GetNodeFromLine(string line, DialogueTreeNode parentNode)
		{
			DialogueTreeNode node = null;

			foreach (var (nodeType, regex) in NodeTypePatterns)
			{
				// Search which type of node this line is
				Match nodeMatch = regex.Match(line);
				if (!nodeMatch.Success) continue;

				node = new(nodeType, parentNode);

				// Save all the captured arguments of this dialogue line
				for (int i = 1; i < nodeMatch.Groups.Count; i++)
				{
					string nodeArgument = nodeMatch.Groups[i].Value;
					node.Data.Add(nodeArgument);
				}
				break;
			}

			return node;
		}

		void AddNodeToChildList(DialogueTreeNode node, List<DialogueTreeNode> children)
		{
			node.SetId(++nodeId);
			children.Add(node);
		}

		void InitializeSceneNextIds(DialogueTree dialogueTree, DialogueTreeScene scene)
		{
			for (int i = scene.MinId; i < scene.MaxId; i++)
			{
				DialogueTreeNode current = dialogueTree.GetNode(i);
				DialogueTreeNode next = dialogueTree.GetNode(i + 1);

				if (current.Parent == null)
				{
					current.NextId = next.Id;
				}
				else if (current.Type == DialogueNodeType.Condition || current.Type == DialogueNodeType.Choice)
				{
					current.NextId = current.MaxId + 1;
				}
				else if (current.Type == DialogueNodeType.ConditionBranch || current.Type == DialogueNodeType.ChoiceBranch)
				{
					current.NextId = current.Parent.MaxId + 1;
				}
				else if (current.Parent.Type == DialogueNodeType.ConditionBranch || current.Parent.Type == DialogueNodeType.ChoiceBranch)
				{
					if (next.Type != DialogueNodeType.ConditionBranch && next.Type != DialogueNodeType.ChoiceBranch)
					{
						current.NextId = next.Id;
					}
					else
					{
						next = dialogueTree.GetNode(current.Parent.Parent.MaxId + 1);
						while (next != null && (next.Type == DialogueNodeType.ConditionBranch || next.Type == DialogueNodeType.ChoiceBranch))
						{
							next = dialogueTree.GetNode(next.Parent.MaxId + 1);
						}

						if (next != null)
							current.NextId = next.Id;
					}
				}
			}
		}
	}
}
