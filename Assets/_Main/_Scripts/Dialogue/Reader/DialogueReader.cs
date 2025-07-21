using History;
using System.Collections;
using UI;

namespace Dialogue
{
	public class DialogueReader
	{
		readonly DialogueContinuePromptUI continuePrompt;
		readonly DialogueStack dialogueStack;
		readonly HistoryManager historyManager;
		readonly DialogueLineReader lineReader;
		readonly DialogueLogicReader logicReader;
		readonly GameState gameState;
		
		bool isReading = false;
		bool isRunning = false;
		bool isWaitingToAdvance = true;
		
		public bool IsRunning { get { return isRunning; } set { isRunning = value; } }
		public bool IsReading { get { return isReading; } set { isReading = value; } }
		public bool IsWaitingToAdvance { get { return isWaitingToAdvance; } set { isWaitingToAdvance = value; } }
		public DialogueStack Stack => dialogueStack;
		public DialogueContinuePromptUI ContinuePrompt => continuePrompt;
		public DialogueLineReader LineReader => lineReader;
		public DialogueLogicReader LogicReader => logicReader;

		public DialogueReader(DialogueManager dialogueManager)
		{
			continuePrompt = dialogueManager.ContinuePrompt;
			historyManager = dialogueManager.History;
			gameState = dialogueManager.State;

			dialogueStack = new();
			lineReader = new(dialogueManager, this);
			logicReader = new(dialogueManager.Logic);
		}

		public IEnumerator ReadImmediate(string dialogueText)
		{
			isReading = false;
			while (lineReader.IsBuildingText) yield return null;

			lineReader.ReadImmediateText(dialogueText);

			while (lineReader.IsBuildingText) yield return null;
			continuePrompt.Show();
		}

		// Read lines directly from dialogue files (each line includes: speaker, dialogue, commands)
		public IEnumerator Read(DialogueReadMode dialogueReadMode)
		{
			lineReader.UpdateTextBuildMode(dialogueReadMode);
			IsRunning = true;

			while (isRunning)
			{
				isReading = IsRunning;

				// Get the next non-null line and remove any blocks that are complete
				string rawLine = dialogueStack.GetCurrentLine();
				
				while (rawLine == null)
				{
					// Don't end the dialogue if there are no lines left - check if the stack was refreshed
					yield return WaitForStackUpdate();

					if (!IsRunning)
					{
						isReading = false;
						yield break;
					}
					
					dialogueStack.Proceed(dialogueStack.GetBlock());
					rawLine = dialogueStack.GetCurrentLine();
				}

				// Cache the block this line belongs to because it might change during this iteration
				DialogueBlock dialogueBlock = dialogueStack.GetBlock();

				// Parse the new line - it will either contain dialogue or logic
				DialogueLine dialogueLine = DialogueParser.Parse(dialogueBlock.FilePath, dialogueBlock.Progress, rawLine, logicReader.LogicManager);

				yield return lineReader.ProcessDialogueLine(dialogueLine);
				yield return logicReader.ProcessLogicSegment(dialogueLine);

				yield return ProcessCompletedLine(dialogueBlock, dialogueLine);

				isReading = false;
				yield return null;
			}
		}

		IEnumerator ProcessCompletedLine(DialogueBlock dialogueBlock, DialogueLine dialogueLine)
		{
			// Mark this line as read
			string dialogueLineId = DialogueUtilities.GetDialogueLineId(dialogueLine.FilePath, dialogueLine.LineNumber);
			if (dialogueLine.Dialogue != null)
			{
				gameState.AddReadLine(dialogueLineId);
				yield return ProcessHistory();
			}

			dialogueStack.Proceed(dialogueBlock);
		}

		IEnumerator WaitForStackUpdate()
		{
			while (isRunning && dialogueStack.IsEmpty)
				yield return ProcessHistory();
		}

		IEnumerator ProcessHistory()
		{
			while (isRunning && isWaitingToAdvance) yield return null;

			if (historyManager.LastAction == HistoryAction.Load)
				yield return historyManager.ApplyNavigationHistory();
		}
	}
}
