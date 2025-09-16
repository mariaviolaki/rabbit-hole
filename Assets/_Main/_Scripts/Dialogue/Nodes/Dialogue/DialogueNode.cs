using Characters;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dialogue
{
	public class DialogueNode : NodeBase
	{
		const char PositionSeparator = ',';
		const char VisualSeparator = ':';
		static readonly Regex PositionRegex = new(@"at\s*\[([0-9.,\s]+)\]", RegexOptions.IgnoreCase);
		static readonly Regex VisualRegex = new(@"\(([^)]+)\)", RegexOptions.IgnoreCase);
		static readonly Regex DialogueSegmentRegex = new(@"\{[ac](?:\s+\d+(?:\.\d+)?)?\}", RegexOptions.IgnoreCase);

		readonly GameProgressManager gameProgressManager;
		readonly DialogueManager dialogueManager;
		readonly DialogueSpeakerHandler speakerHandler;
		readonly DialogueTextHandler textHandler;

		readonly List<DialogueTextSegment> segments = new();
		string shortName;
		string visual;
		SpriteLayerType layerType = SpriteLayerType.None;
		float xPos = float.NaN;
		float yPos = float.NaN;
		
		public DialogueNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
			gameProgressManager = flowController.VN.Game.Progress;
			dialogueManager = flowController.Dialogue;
			speakerHandler = flowController.DialogueSpeaker;
			textHandler = flowController.DialogueText;
		}

		public override void StartExecution()
		{
			if (IsExecuting) return;
			base.StartExecution();

			ParseTreeNode();
			executionCoroutine = flowController.Dialogue.StartCoroutine(ExecuteLogic());
		}

		protected override void ParseTreeNode()
		{
			shortName = string.IsNullOrWhiteSpace(treeNode.Data[0]) ? null : treeNode.Data[0].Trim();

			ExtractDialogueParameters(treeNode.Data[1].Trim());
			ExtractDialogueSegments(treeNode.Data[2].Trim());
		}

		protected override IEnumerator ExecuteLogic()
		{
			yield return base.ExecuteLogic();

			if (segments.Count > 0)
			{
				List<IEnumerator> processes = new()
				{
					speakerHandler.SetSpeaker(shortName, layerType, visual, xPos, yPos),
					textHandler.DisplayDialogue(segments)
				};

				yield return Utilities.RunConcurrentProcesses(dialogueManager, processes);
			}

			// Mark this line as read
			string dialogueLineId = TreeNodeUtilities.GetDialogueNodeId(flowController.CurrentSceneName, flowController.CurrentNodeId);
			gameProgressManager.AddReadLine(dialogueLineId);

			executionCoroutine = null;
			flowController.ProceedToNode(treeNode.NextId);
		}

		public override void SpeedUpExecution()
		{
			base.SpeedUpExecution();
			textHandler.ForceComplete();
		}

		// Optional commands between the speaker and the dialogue text
		void ExtractDialogueParameters(string rawParameters)
		{
			if (string.IsNullOrWhiteSpace(rawParameters)) return;

			// If a position was specified, save it (y is optional)
			Match positionMatch = PositionRegex.Match(rawParameters);
			if (positionMatch.Success)
			{
				string[] positionParts = positionMatch.Groups[1].Value.Split(PositionSeparator, StringSplitOptions.RemoveEmptyEntries);

				if (positionParts.Length > 0 && float.TryParse(positionParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
					xPos = x;
				if (positionParts.Length > 1 && float.TryParse(positionParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
					yPos = y;
			}

			// If a sprite name or model expression was specified, save it (layer type is optional)
			Match visualMatch = VisualRegex.Match(rawParameters);
			if (visualMatch.Success)
			{
				string[] visualParts = visualMatch.Groups[1].Value.Trim().Split(VisualSeparator, StringSplitOptions.RemoveEmptyEntries);

				if (visualParts.Length == 1)
				{
					visual = visualParts[0];
				}
				else if (visualParts.Length > 1)
				{
					layerType = GetLayerFromText(visualParts[0].Trim());
					visual = visualParts[1].Trim();
				}
			}
		}

		void ExtractDialogueSegments(string dialogueText)
		{
			MatchCollection segmentMatches = DialogueSegmentRegex.Matches(dialogueText);

			// Segment separators are not necessary in the text - dialogue text might be the only segment
			int firstSegmentEnd = segmentMatches.Count > 0 ? segmentMatches[0].Index : dialogueText.Length;
			DialogueTextSegment firstSegment = new(dialogueText.Substring(0, firstSegmentEnd), SegmentStartMode.None);
			segments.Add(firstSegment);

			// Split the dialogue into multiple text segments, each displayed separately
			if (segmentMatches.Count > 0)
			{
				for (int i = 0; i < segmentMatches.Count; i++)
				{
					Match match = segmentMatches[i];
					Match nextMatch = i + 1 == segmentMatches.Count ? null : segmentMatches[i + 1];

					segments.Add(GetSegmentBetweenMatches(dialogueText, match, nextMatch));
				}
			}
		}

		DialogueTextSegment GetSegmentBetweenMatches(string rawText, Match match, Match nextMatch)
		{
			// The text to be displayed
			int textStart = match.Index + match.Length;
			int textLength = nextMatch == null ? rawText.Length - textStart : nextMatch.Index - textStart;
			string text = rawText.Substring(textStart, textLength);

			// How this text will be displayed
			string startModeText = match.Value.Substring(1, match.Length - 2);
			string[] startModeParams = startModeText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			SegmentStartMode startMode = GetStartModeFromText(startModeParams);

			// Optionally, wait for a few seconds before automatically showing the text
			float waitTime = 0f;
			if (startModeParams.Length > 1)
				float.TryParse(startModeParams[1], NumberStyles.Float, CultureInfo.InvariantCulture, out waitTime);

			return new DialogueTextSegment(text, startMode, waitTime);
		}

		SpriteLayerType GetLayerFromText(string layerText)
		{
			if (Enum.TryParse(typeof(SpriteLayerType), layerText, ignoreCase: true, out object layer))
				return (SpriteLayerType)layer;

			return SpriteLayerType.None;
		}

		SegmentStartMode GetStartModeFromText(string[] startModeParams)
		{
			string startModeKeyword = startModeParams[0];

			if (startModeParams.Length == 1)
			{
				// No wait time was defined - so we wait for player input
				if (startModeKeyword == "c") return SegmentStartMode.InputClear;
				else if (startModeKeyword == "a") return SegmentStartMode.InputAppend;
			}
			else
			{
				// We wait for a set amount of time before showing the next text segment
				if (startModeKeyword == "c") return SegmentStartMode.AutoClear;
				else if (startModeKeyword == "a") return SegmentStartMode.AutoAppend;
			}

			return SegmentStartMode.None;
		}
	}
}
